﻿//------------------------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Forms;
using System.ComponentModel;

namespace Pilgrim.Template.Dialog
{
	internal partial class DataConnectionAdvancedDialog : Form
	{
		public DataConnectionAdvancedDialog()
		{
			InitializeComponent();

			// Make sure we handle a user preference change
			if (this.components == null)
			{
				this.components = new System.ComponentModel.Container();
			}
			this.components.Add(new UserPreferenceChangedHandler(this));
		}

		public DataConnectionAdvancedDialog(IDataConnectionProperties connectionProperties, DataConnectionDialog mainDialog)
			: this()
		{
			Debug.Assert(connectionProperties != null);
			Debug.Assert(mainDialog != null);

			_savedConnectionString = connectionProperties.ToFullString();

			this.propertyGrid.SelectedObject = connectionProperties;

			_mainDialog = mainDialog;
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			ConfigureTextBox();
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
			propertyGrid.Focus();
		}

		protected override void OnFontChanged(EventArgs e)
		{
			base.OnFontChanged(e);

			this.textBox.Width = this.propertyGrid.Width;
		}

		protected override void OnHelpRequested(HelpEventArgs hevent)
		{
			// Get the active control
			Control activeControl = this;
			ContainerControl containerControl = null;
			while ((containerControl = activeControl as ContainerControl) != null &&
				containerControl != propertyGrid &&
				containerControl.ActiveControl != null)
			{
				activeControl = containerControl.ActiveControl;
			}

			// Figure out the context
			DataConnectionDialogContext context = DataConnectionDialogContext.Advanced;
			if (activeControl == propertyGrid)
			{
				context = DataConnectionDialogContext.AdvancedPropertyGrid;
			}
			if (activeControl == textBox)
			{
				context = DataConnectionDialogContext.AdvancedTextBox;
			}
			if (activeControl == okButton)
			{
				context = DataConnectionDialogContext.AdvancedOkButton;
			}
			if (activeControl == cancelButton)
			{
				context = DataConnectionDialogContext.AdvancedCancelButton;
			}

			// Call OnContextHelpRequested
			ContextHelpEventArgs e = new ContextHelpEventArgs(context, hevent.MousePos);
			_mainDialog.OnContextHelpRequested(e);
			hevent.Handled = e.Handled;
			if (!e.Handled)
			{
				base.OnHelpRequested(hevent);
			}
		}

		protected override void WndProc(ref Message m)
		{
			if (_mainDialog.TranslateHelpButton && HelpUtils.IsContextHelpMessage(ref m))
			{
				// Force the ? in the title bar to invoke the help topic
				HelpUtils.TranslateContextHelpMessage(this, ref m);
			}
			base.WndProc(ref m);
		}

		internal class SpecializedPropertyGrid : PropertyGrid
		{
			public SpecializedPropertyGrid()
			{
				_contextMenu = new ContextMenuStrip();

				_contextMenu.Items.AddRange(new ToolStripItem[] {
					new ToolStripMenuItem(),
					new ToolStripSeparator(),
					new ToolStripMenuItem(),
					new ToolStripMenuItem(),
					new ToolStripSeparator(),
					new ToolStripMenuItem()
				});
				_contextMenu.Items[0].Text = Strings.DataConnectionAdvancedDialog_Reset;
				_contextMenu.Items[0].Click += new EventHandler(ResetProperty);
				_contextMenu.Items[2].Text = Strings.DataConnectionAdvancedDialog_Add;
				_contextMenu.Items[2].Click += new EventHandler(AddProperty);
				_contextMenu.Items[3].Text = Strings.DataConnectionAdvancedDialog_Remove;
				_contextMenu.Items[3].Click += new EventHandler(RemoveProperty);
				_contextMenu.Items[5].Text = Strings.DataConnectionAdvancedDialog_Description;
				_contextMenu.Items[5].Click += new EventHandler(ToggleDescription);
				(_contextMenu.Items[5] as ToolStripMenuItem).Checked = this.HelpVisible;
				_contextMenu.Opened += new EventHandler(SetupContextMenu);

				this.ContextMenuStrip = _contextMenu;
				this.DrawFlatToolbar = true;
				this.Size = new Size(270, 250); // magic numbers, but a reasonable starting point
				this.MinimumSize = Size;
			}

			protected override void OnHandleCreated(EventArgs e)
			{
				ProfessionalColorTable colorTable = (ParentForm != null && ParentForm.Site != null) ? ParentForm.Site.GetService(typeof(ProfessionalColorTable)) as ProfessionalColorTable : null;
				if (colorTable != null)
				{
					this.ToolStripRenderer = new ToolStripProfessionalRenderer(colorTable);
				}
				base.OnHandleCreated(e);
			}

			protected override void OnFontChanged(EventArgs e)
			{
				base.OnFontChanged(e);
				this.LargeButtons = (this.Font.SizeInPoints >= 15.0);
			}

			protected override void WndProc(ref Message m)
			{
				switch (m.Msg)
				{
					case NativeMethods.WM_SETFOCUS:
						// Make sure the property grid view has proper focus
						Focus();
						((System.Windows.Forms.ComponentModel.Com2Interop.IComPropertyBrowser)this).HandleF4();
						break;
				}
				base.WndProc(ref m);
			}

			private void SetupContextMenu(object sender, System.EventArgs e)
			{
				// Decide if reset should be enabled
				_contextMenu.Items[0].Enabled = (SelectedGridItem.GridItemType == GridItemType.Property);
				if (_contextMenu.Items[0].Enabled && SelectedGridItem.PropertyDescriptor != null)
				{
					object propertyOwner = SelectedObject;
					if (SelectedObject is ICustomTypeDescriptor)
					{
						propertyOwner = (SelectedObject as ICustomTypeDescriptor).GetPropertyOwner(SelectedGridItem.PropertyDescriptor);
					}
					_contextMenu.Items[0].Enabled = _contextMenu.Items[3].Enabled = SelectedGridItem.PropertyDescriptor.CanResetValue(propertyOwner);
				}

				// Decide if we are allowed to add/remove custom properties
				_contextMenu.Items[2].Visible = _contextMenu.Items[3].Visible = (SelectedObject as IDataConnectionProperties).IsExtensible;
				if (_contextMenu.Items[3].Visible)
				{
					_contextMenu.Items[3].Enabled = (SelectedGridItem.GridItemType == GridItemType.Property);
					if (_contextMenu.Items[3].Enabled && SelectedGridItem.PropertyDescriptor != null)
					{
						_contextMenu.Items[3].Enabled = !SelectedGridItem.PropertyDescriptor.IsReadOnly;
					}
				}

				// Hide the first separator if there is no need for it
				_contextMenu.Items[1].Visible = (_contextMenu.Items[2].Visible || _contextMenu.Items[3].Visible);
			}

			private void ResetProperty(object sender, System.EventArgs e)
			{
				object oldValue = SelectedGridItem.Value;
				object propertyOwner = SelectedObject;
				if (SelectedObject is ICustomTypeDescriptor)
				{
					propertyOwner = (SelectedObject as ICustomTypeDescriptor).GetPropertyOwner(SelectedGridItem.PropertyDescriptor);
				}
				SelectedGridItem.PropertyDescriptor.ResetValue(propertyOwner);
				Refresh();
				OnPropertyValueChanged(new PropertyValueChangedEventArgs(SelectedGridItem, oldValue));
			}

			private void AddProperty(object sender, System.EventArgs e)
			{
				DataConnectionDialog mainDialog = ParentForm as DataConnectionDialog;
				if (mainDialog == null)
				{
					Debug.Assert(ParentForm is DataConnectionAdvancedDialog);
					mainDialog = (ParentForm as DataConnectionAdvancedDialog)._mainDialog;
					Debug.Assert(mainDialog != null);
				}
				AddPropertyDialog dialog = new AddPropertyDialog(mainDialog);
				try
				{
					if (ParentForm.Container != null)
					{
						ParentForm.Container.Add(dialog);
					}
					DialogResult result = dialog.ShowDialog(ParentForm);
					if (result == DialogResult.OK)
					{
						(SelectedObject as IDataConnectionProperties).Add(dialog.PropertyName);
						Refresh();
						GridItem rootItem = SelectedGridItem;
						while (rootItem.Parent != null)
						{
							rootItem = rootItem.Parent;
						}
						GridItem newItem = LocateGridItem(rootItem, dialog.PropertyName);
						if (newItem != null)
						{
							SelectedGridItem = newItem;
						}
					}
				}
				finally
				{
					if (ParentForm.Container != null)
					{
						ParentForm.Container.Remove(dialog);
					}
					dialog.Dispose();
				}
			}

			private void RemoveProperty(object sender, System.EventArgs e)
			{
				(SelectedObject as IDataConnectionProperties).Remove(SelectedGridItem.Label);
				Refresh();
				OnPropertyValueChanged(new PropertyValueChangedEventArgs(null, null));
			}

			private void ToggleDescription(object sender, System.EventArgs e)
			{
				this.HelpVisible = !this.HelpVisible;
				(_contextMenu.Items[5] as ToolStripMenuItem).Checked = !(_contextMenu.Items[5] as ToolStripMenuItem).Checked;
			}

			private GridItem LocateGridItem(GridItem currentItem, string propertyName)
			{
				if (currentItem.GridItemType == GridItemType.Property &&
					currentItem.Label.Equals(propertyName, StringComparison.CurrentCulture))
				{
					return currentItem;
				}

				GridItem foundItem = null;
				foreach (GridItem childItem in currentItem.GridItems)
				{
					foundItem = LocateGridItem(childItem, propertyName);
					if (foundItem != null)
					{
						break;
					}
				}

				return foundItem;
			}

			private ContextMenuStrip _contextMenu;
		}

		private void SetTextBox(object s, PropertyValueChangedEventArgs e)
		{
			ConfigureTextBox();
		}

		private void ConfigureTextBox()
		{
			if (this.propertyGrid.SelectedObject is IDataConnectionProperties)
			{
				try
				{
					this.textBox.Text = (this.propertyGrid.SelectedObject as IDataConnectionProperties).ToDisplayString();
				}
				catch
				{
					this.textBox.Text = null;
				}
			}
			else
			{
				this.textBox.Text = null;
			}
		}

		private void RevertProperties(object sender, EventArgs e)
		{
			try
			{
				(this.propertyGrid.SelectedObject as IDataConnectionProperties).Parse(_savedConnectionString);
			}
			catch { }
		}

		private string _savedConnectionString;
		private DataConnectionDialog _mainDialog;
	}
}
