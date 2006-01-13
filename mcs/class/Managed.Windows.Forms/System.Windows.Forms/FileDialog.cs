// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// Copyright (c) 2004 Novell, Inc. (http://www.novell.com)
//
// Authors:
//
//  Alexander Olk	xenomorph2@onlinehome.de
//

// NOT COMPLETE - work in progress

using System;
using System.Drawing;
using System.ComponentModel;
using System.Resources;
using System.IO;
using System.Collections;
using System.Collections.Specialized;
using System.Xml;

namespace System.Windows.Forms
{
	[DefaultProperty( "FileName" )]
	[DefaultEvent( "FileOk" )]
	public abstract class FileDialog : CommonDialog
	{
		protected static readonly object EventFileOk = new object ();

		internal enum FileDialogType
		{
			OpenFileDialog,
			SaveFileDialog
		}
		
		internal FileDialogPanel fileDialogPanel;
		
		private bool addExtension = true;
		internal bool checkFileExists = false;
		private bool checkPathExists = true;
		private string defaultExt = "";
		private bool dereferenceLinks = true;
		private string fileName = "";
		private string[] fileNames;
		private string filter;
		private int filterIndex = 1;
		private string initialDirectory = "";
		private bool restoreDirectory = false;
		private bool showHelp = false;
		private string title = "";
		private bool validateNames = true;
		
		//protected bool readOnlyChecked = false;
		
		internal string openSaveButtonText;
		internal string searchSaveLabelText;
		internal bool showReadOnly = false;
		internal bool readOnlyChecked = false;
		internal bool multiSelect = false;
		internal bool createPrompt = false;
		internal bool overwritePrompt = true;
		
		private bool showHiddenFiles = false;
		
		internal FileDialogType fileDialogType;
		
		internal FileDialog( ) : base()
		{
			form.MaximizeBox = true;
		}
		
		[DefaultValue(true)]
		public bool AddExtension
		{
			get {
				return addExtension;
			}
			
			set {
				addExtension = value;
			}
		}
		
		[DefaultValue(false)]
		public virtual bool CheckFileExists
		{
			get {
				return checkFileExists;
			}
			
			set {
				checkFileExists = value;
			}
		}
		
		[DefaultValue(true)]
		public bool CheckPathExists
		{
			get {
				return checkPathExists;
			}
			
			set {
				checkPathExists = value;
			}
		}
		
		[DefaultValue("")]
		public string DefaultExt
		{
			get {
				return defaultExt;
			}
			
			set {
				defaultExt = value;
				
				// if there is a dot remove it and everything before it
				if ( defaultExt.LastIndexOf( '.' ) != - 1 )
				{
					string[] split = defaultExt.Split( new char[] { '.' } );
					defaultExt = split[ split.Length - 1 ];
				}
			}
		}
		
		// in MS.NET it doesn't make a difference if
		// DerefenceLinks is true or false
		// if the selected file is a link FileDialog
		// always returns the link
		[DefaultValue(true)]
		public bool DereferenceLinks
		{
			get {
				return dereferenceLinks;
			}
			
			set {
				dereferenceLinks = value;
			}
		}
		
		[DefaultValue("")]
		public string FileName
		{
			get {
				return fileName;
			}
			
			set {
				fileName = value;
			}
		}
		
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string[] FileNames
		{
			get {
				if ( multiSelect )
					return fileNames;
				
				return null;
			}
		}
		
		[DefaultValue("")]
		[Localizable(true)]
		public string Filter
		{
			get {
				return filter;
			}
			
			set {
				if ( value == null )
					throw new NullReferenceException( "Filter" );
				
				filter = value;
				
				fileFilter = new FileFilter( filter );
				
				fileDialogPanel.UpdateFilters( );
			}
		}
		
		[DefaultValue(1)]
		public int FilterIndex
		{
			get {
				return filterIndex;
			}
			
			set {
				filterIndex = value;
			}
		}
		
		[DefaultValue("")]
		public string InitialDirectory
		{
			get {
				return initialDirectory;
			}
			
			set {
				if (Directory.Exists (value)) {
					initialDirectory = value;
				
					fileDialogPanel.ChangeDirectory( null, initialDirectory );
				}
			}
		}
		
		[DefaultValue(false)]
		public bool RestoreDirectory
		{
			get {
				return restoreDirectory;
			}
			
			set {
				restoreDirectory = value;
			}
		}
		
		[DefaultValue(false)]
		public bool ShowHelp
		{
			get {
				return showHelp;
			}
			
			set {
				showHelp = value;
				fileDialogPanel.ResizeAndRelocateForHelpOrReadOnly( );
			}
		}
		
		[DefaultValue("")]
		[Localizable(true)]
		public string Title
		{
			get {
				return title;
			}
			
			set {
				title = value;
				
				form.Text = title;
			}
		}
		
		// this one is a hard one ;)
		// Win32 filename:
		// - up to MAX_PATH characters (windef.h) = 260
		// - no trailing dots or spaces
		// - case preserving
		// - etc...
		// NTFS/Posix filename:
		// - up to 32,768 Unicode characters
		// - trailing periods or spaces
		// - case sensitive
		// - etc...
		[DefaultValue(true)]
		public bool ValidateNames
		{
			get {
				return validateNames;
			}
			
			set {
				validateNames = value;
			}
		}
		
		internal string OpenSaveButtonText
		{
			set {
				openSaveButtonText = value;
			}
			
			get {
				return openSaveButtonText;
			}
		}
		
		internal string SearchSaveLabelText
		{
			set {
				searchSaveLabelText = value;
			}
			
			get {
				return searchSaveLabelText;
			}
		}
		
		internal virtual bool ShowReadOnly
		{
			set {
				showReadOnly = value;
				fileDialogPanel.ResizeAndRelocateForHelpOrReadOnly( );
			}
			
			get {
				return showReadOnly;
			}
		}
		
		internal virtual bool ReadOnlyChecked
		{
			set {
				readOnlyChecked = value;
				fileDialogPanel.CheckBox.Checked = value;
			}
			
			get {
				return readOnlyChecked;
			}
		}
		
		internal virtual bool Multiselect
		{
			set {
				multiSelect = value;
				fileDialogPanel.MultiSelect = value;
			}
			
			get {
				return multiSelect;
			}
		}
		
		// extension to MS.NET framework...
		// Must keep this internal, otherwise our signature doesn't match MS
		internal bool ShowHiddenFiles
		{
			set {
				showHiddenFiles = value;
			}
			
			get {
				return showHiddenFiles;
			}
		}
		
		internal virtual bool CreatePrompt
		{
			set {
				createPrompt = value;
			}
			
			get {
				return createPrompt;
			}
		}
		
		internal virtual bool OverwritePrompt
		{
			set {
				overwritePrompt = value;
			}
			
			get {
				return overwritePrompt;
			}
		}
		
		internal FileFilter FileFilter
		{
			set {
				fileFilter = value;
			}
			
			get {
				return fileFilter;
			}
		}
		
		public override void Reset( )
		{
			addExtension = true;
			checkFileExists = false;
			checkPathExists = true;
			defaultExt = "";
			dereferenceLinks = true;
			fileName = "";
			fileNames = null;
			Filter = "";
			filterIndex = 1;
			initialDirectory = "";
			restoreDirectory = false;
			ShowHelp = false;
			Title = "";
			validateNames = true;
			
			fileDialogPanel.UpdateFilters( );
		}
		
		public override string ToString( )
		{
			return base.ToString( );
		}

		public event CancelEventHandler FileOk {
			add { Events.AddHandler (EventFileOk, value); }
			remove { Events.RemoveHandler (EventFileOk, value); }
		}

		protected virtual IntPtr Instance {
			get {
				if (fileDialogPanel == null)
					return IntPtr.Zero;
				return fileDialogPanel.Handle;
			}
		}

		// This is just for internal use with MSs version, so it doesn't need to be implemented
		// as it can't really be accessed anyways
		protected int Options {
			get { return -1; }
		}

		[MonoTODO]
		protected  override IntPtr HookProc( IntPtr hWnd, int msg, IntPtr wparam, IntPtr lparam )
		{
			throw new NotImplementedException( );
		}
		
		protected void OnFileOk( CancelEventArgs e )
		{
			CancelEventHandler fo = (CancelEventHandler) Events [EventFileOk];
			if (fo != null)
				fo (this, e);
		}
		
		[MonoTODO]
		protected  override bool RunDialog( IntPtr hWndOwner )
		{
			form.Controls.Add( fileDialogPanel );
			
			return true;
		}
		
		internal void SendHelpRequest( EventArgs e )
		{
			OnHelpRequest( e );
		}
		
		internal void SetFilenames( string[] filenames )
		{
			fileNames = filenames;
		}
		
		internal FileFilter fileFilter;
		
		internal class FileDialogPanel : Panel
		{
			private Button cancelButton;
			private ToolBarButton upToolBarButton;
			private PopupButtonPanel popupButtonPanel;
			private Button openSaveButton;
			private Button helpButton;
			private Label fileTypeLabel;
			private ToolBarButton menueToolBarButton;
			private ContextMenu menueToolBarButtonContextMenu;
			private ToolBar smallButtonToolBar;
			private DirComboBox dirComboBox;
			private ComboBox fileNameComboBox;
			private Label fileNameLabel;
			private MWFFileView mwfFileView;
			private Label searchSaveLabel;
			private ToolBarButton newdirToolBarButton;
			private ToolBarButton backToolBarButton;
			private ComboBox fileTypeComboBox;
			private ImageList imageListTopToolbar;
			private ContextMenu contextMenu;
			private CheckBox checkBox;
			
			internal FileDialog fileDialog;
			
			private string currentDirectoryName;
			
			internal string currentFileName = "";
			
			// store current directoryInfo
			private DirectoryInfo currentDirectoryInfo;
			
			// store DirectoryInfo for backButton
			internal Stack directoryStack = new Stack();
			
			private MenuItem previousCheckedMenuItem;
			
			private bool multiSelect = false;
			
			private string restoreDirectory = "";
			
			private bool show_special_case = false;
			
			internal static readonly string recently_string = "[recently/recently]";
			
			private string current_special_case;
			
			public FileDialogPanel( FileDialog fileDialog )
			{
				this.fileDialog = fileDialog;
				
				fileTypeComboBox = new ComboBox( );
				backToolBarButton = new ToolBarButton( );
				newdirToolBarButton = new ToolBarButton( );
				searchSaveLabel = new Label( );
				mwfFileView = new MWFFileView( );
				fileNameLabel = new Label( );
				fileNameComboBox = new ComboBox( );
				dirComboBox = new DirComboBox( );
				smallButtonToolBar = new ToolBar( );
				menueToolBarButton = new ToolBarButton( );
				fileTypeLabel = new Label( );
				openSaveButton = new Button( );
				fileDialog.form.AcceptButton = openSaveButton;
				helpButton = new Button( );
				popupButtonPanel = new PopupButtonPanel( this );
				upToolBarButton = new ToolBarButton( );
				cancelButton = new Button( );
				fileDialog.form.CancelButton = cancelButton;
				imageListTopToolbar = new ImageList( );
				menueToolBarButtonContextMenu = new ContextMenu( );
				contextMenu = new ContextMenu( );
				checkBox = new CheckBox( );
				
				SuspendLayout( );
				
				//imageListTopToolbar
				imageListTopToolbar.ColorDepth = ColorDepth.Depth32Bit;
				imageListTopToolbar.ImageSize = new Size( 16, 16 ); // 16, 16
				imageListTopToolbar.Images.Add( (Image)Locale.GetResource( "back_arrow" ) );
				imageListTopToolbar.Images.Add( (Image)Locale.GetResource( "folder_arrow_up" ) );
				imageListTopToolbar.Images.Add( (Image)Locale.GetResource( "folder_star" ) );
				imageListTopToolbar.Images.Add( (Image)Locale.GetResource( "window" ) );
				imageListTopToolbar.TransparentColor = Color.Transparent;
				
				// searchLabel
				searchSaveLabel.FlatStyle = FlatStyle.System;
				searchSaveLabel.Location = new Point( 7, 8 );
				searchSaveLabel.Size = new Size( 72, 21 );
				searchSaveLabel.TabIndex = 0;
				searchSaveLabel.Text = fileDialog.SearchSaveLabelText;
				searchSaveLabel.TextAlign = ContentAlignment.MiddleRight;
				
				// dirComboBox
				dirComboBox.Anchor = ( (AnchorStyles)( ( ( AnchorStyles.Top | AnchorStyles.Left ) | AnchorStyles.Right ) ) );
				dirComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
				dirComboBox.Location = new Point( 99, 8 );
				dirComboBox.Size = new Size( 260, 21 );
				dirComboBox.TabIndex = 1;
				
				// smallButtonToolBar
				smallButtonToolBar.Anchor = ( (AnchorStyles)( ( AnchorStyles.Top | AnchorStyles.Right ) ) );
				smallButtonToolBar.Appearance = ToolBarAppearance.Flat;
				smallButtonToolBar.AutoSize = false;
				smallButtonToolBar.Buttons.AddRange( new ToolBarButton[] {
									    backToolBarButton,
									    upToolBarButton,
									    newdirToolBarButton,
									    menueToolBarButton} );
				smallButtonToolBar.ButtonSize = new Size( 21, 16 ); // 21, 16
				smallButtonToolBar.Divider = false;
				smallButtonToolBar.Dock = DockStyle.None;
				smallButtonToolBar.DropDownArrows = true;
				smallButtonToolBar.ImageList = imageListTopToolbar;
				smallButtonToolBar.Location = new Point( 372, 8 );
				smallButtonToolBar.ShowToolTips = true;
				smallButtonToolBar.Size = new Size( 110, 20 );
				smallButtonToolBar.TabIndex = 2;
				smallButtonToolBar.TextAlign = ToolBarTextAlign.Right;
				
				// buttonPanel
				popupButtonPanel.Dock = DockStyle.None;
				popupButtonPanel.Location = new Point( 7, 37 );
				popupButtonPanel.TabIndex = 3;
				
				// mwfFileView
				mwfFileView.Anchor = ( (AnchorStyles)( ( ( ( AnchorStyles.Top | AnchorStyles.Bottom ) | AnchorStyles.Left ) | AnchorStyles.Right ) ) );
				mwfFileView.Location = new Point( 99, 37 );
				mwfFileView.Size = new Size( 449, 282 );
				mwfFileView.Columns.Add( " Name", 170, HorizontalAlignment.Left );
				mwfFileView.Columns.Add( "Size ", 80, HorizontalAlignment.Right );
				mwfFileView.Columns.Add( " Type", 100, HorizontalAlignment.Left );
				mwfFileView.Columns.Add( " Last Access", 150, HorizontalAlignment.Left );
				mwfFileView.AllowColumnReorder = true;
				mwfFileView.MultiSelect = false;
				mwfFileView.TabIndex = 4;
				mwfFileView.FilterIndex = fileDialog.FilterIndex;
				
				// fileNameLabel
				fileNameLabel.Anchor = ( (AnchorStyles)( ( AnchorStyles.Bottom | AnchorStyles.Left ) ) );
				fileNameLabel.FlatStyle = FlatStyle.System;
				fileNameLabel.Location = new Point( 102, 330 );
				fileNameLabel.Size = new Size( 70, 21 );
				fileNameLabel.TabIndex = 5;
				fileNameLabel.Text = "Filename:";
				fileNameLabel.TextAlign = ContentAlignment.MiddleLeft;
				
				// fileNameComboBox
				fileNameComboBox.Anchor = ( (AnchorStyles)( ( ( AnchorStyles.Bottom | AnchorStyles.Left ) | AnchorStyles.Right ) ) );
				fileNameComboBox.Location = new Point( 195, 330 );
				fileNameComboBox.Size = new Size( 245, 21 );
				fileNameComboBox.TabIndex = 6;
				fileNameComboBox.Items.Add( " " );
				
				// fileTypeLabel
				fileTypeLabel.Anchor = ( (AnchorStyles)( ( AnchorStyles.Bottom | AnchorStyles.Left ) ) );
				fileTypeLabel.FlatStyle = FlatStyle.System;
				fileTypeLabel.Location = new Point( 102, 356 );
				fileTypeLabel.Size = new Size( 70, 21 );
				fileTypeLabel.TabIndex = 7;
				fileTypeLabel.Text = "Filetype:";
				fileTypeLabel.TextAlign = ContentAlignment.MiddleLeft;
				
				// fileTypeComboBox
				fileTypeComboBox.Anchor = ( (AnchorStyles)( ( ( AnchorStyles.Bottom | AnchorStyles.Left ) | AnchorStyles.Right ) ) );
				fileTypeComboBox.Location = new Point( 195, 356 );
				fileTypeComboBox.Size = new Size( 245, 21 );
				fileTypeComboBox.TabIndex = 8;
				
				// backToolBarButton
				backToolBarButton.ImageIndex = 0;
				backToolBarButton.Enabled = false;
				backToolBarButton.Style = ToolBarButtonStyle.ToggleButton;
				
				// upToolBarButton
				upToolBarButton.ImageIndex = 1;
				upToolBarButton.Style = ToolBarButtonStyle.ToggleButton;
				
				// newdirToolBarButton
				newdirToolBarButton.ImageIndex = 2;
				newdirToolBarButton.Style = ToolBarButtonStyle.ToggleButton;
				
				// menueToolBarButton
				menueToolBarButton.ImageIndex = 3;
				menueToolBarButton.DropDownMenu = menueToolBarButtonContextMenu;
				menueToolBarButton.Style = ToolBarButtonStyle.DropDownButton;
				
				// menueToolBarButtonContextMenu
				MenuItem mi = new MenuItem( "Small Icon", new EventHandler( OnClickMenuToolBarContextMenu ) );
				menueToolBarButtonContextMenu.MenuItems.Add( mi );
				mi = new MenuItem( "Tiles", new EventHandler( OnClickMenuToolBarContextMenu ) );
				menueToolBarButtonContextMenu.MenuItems.Add( mi );
				mi = new MenuItem( "Large Icon", new EventHandler( OnClickMenuToolBarContextMenu ) );
				menueToolBarButtonContextMenu.MenuItems.Add( mi );
				mi = new MenuItem( "List", new EventHandler( OnClickMenuToolBarContextMenu ) );
				mi.Checked = true;
				previousCheckedMenuItem = mi;
				menueToolBarButtonContextMenu.MenuItems.Add( mi );
				mi = new MenuItem( "Details", new EventHandler( OnClickMenuToolBarContextMenu ) );
				menueToolBarButtonContextMenu.MenuItems.Add( mi );
				
				// contextMenu
				mi = new MenuItem( "Show hidden files", new EventHandler( OnClickContextMenu ) );
				mi.Checked = fileDialog.ShowHiddenFiles;
				mwfFileView.ShowHiddenFiles = fileDialog.ShowHiddenFiles;
				contextMenu.MenuItems.Add( mi );
				
				// openSaveButton
				openSaveButton.Anchor = ( (AnchorStyles)( ( AnchorStyles.Bottom | AnchorStyles.Right ) ) );
				openSaveButton.FlatStyle = FlatStyle.System;
				openSaveButton.Location = new Point( 475, 330 );
				openSaveButton.Size = new Size( 72, 21 );
				openSaveButton.TabIndex = 9;
				openSaveButton.Text = fileDialog.OpenSaveButtonText;
				openSaveButton.FlatStyle = FlatStyle.System;
				
				// cancelButton
				cancelButton.Anchor = ( (AnchorStyles)( ( AnchorStyles.Bottom | AnchorStyles.Right ) ) );
				cancelButton.FlatStyle = FlatStyle.System;
				cancelButton.Location = new Point( 475, 356 );
				cancelButton.Size = new Size( 72, 21 );
				cancelButton.TabIndex = 10;
				cancelButton.Text = "Cancel";
				cancelButton.FlatStyle = FlatStyle.System;
				
				// helpButton
				helpButton.Anchor = ( (AnchorStyles)( ( AnchorStyles.Bottom | AnchorStyles.Right ) ) );
				helpButton.FlatStyle = FlatStyle.System;
				helpButton.Location = new Point( 475, 350 );
				helpButton.Size = new Size( 72, 21 );
				helpButton.TabIndex = 11;
				helpButton.Text = "Help";
				helpButton.FlatStyle = FlatStyle.System;
				
				// checkBox
				checkBox.Anchor = ( (AnchorStyles)( ( ( AnchorStyles.Bottom | AnchorStyles.Left ) | AnchorStyles.Right ) ) );
				checkBox.Text = "Open Readonly";
				checkBox.Location = new Point( 195, 350 );
				checkBox.Size = new Size( 245, 21 );
				checkBox.FlatStyle = FlatStyle.System;
				checkBox.TabIndex = 12;
				
				ClientSize = new Size( 554, 405 ); // 384
				
				ContextMenu = contextMenu;
				
				Dock = DockStyle.Fill;
				
				Controls.Add( smallButtonToolBar );
				Controls.Add( cancelButton );
				Controls.Add( openSaveButton );
				Controls.Add( mwfFileView );
				Controls.Add( fileTypeLabel );
				Controls.Add( fileNameLabel );
				Controls.Add( fileTypeComboBox );
				Controls.Add( fileNameComboBox );
				Controls.Add( dirComboBox );
				Controls.Add( searchSaveLabel );
				Controls.Add( popupButtonPanel );
				
				ResumeLayout( false );
				
				currentDirectoryName = Environment.CurrentDirectory;
				
				currentDirectoryInfo = new DirectoryInfo( currentDirectoryName );
				
				dirComboBox.CurrentPath = currentDirectoryName;
				
				if ( fileDialog.RestoreDirectory )
					restoreDirectory = currentDirectoryName;
				
				mwfFileView.UpdateFileView( currentDirectoryInfo );
				
				openSaveButton.Click += new EventHandler( OnClickOpenSaveButton );
				cancelButton.Click += new EventHandler( OnClickCancelButton );
				helpButton.Click += new EventHandler( OnClickHelpButton );
				
				smallButtonToolBar.ButtonClick += new ToolBarButtonClickEventHandler( OnClickSmallButtonToolBar );
				
				fileTypeComboBox.SelectedIndexChanged += new EventHandler( OnSelectedIndexChangedFileTypeComboBox );
				
				mwfFileView.SelectedFileChanged += new EventHandler( OnSelectedFileChangedFileView );
				mwfFileView.DirectoryChanged += new EventHandler( OnDirectoryChangedFileView );
				mwfFileView.ForceDialogEnd += new EventHandler( OnForceDialogEndFileView );
				mwfFileView.SelectedFilesChanged += new EventHandler( OnSelectedFilesChangedFileView );
				
				dirComboBox.DirectoryChanged += new EventHandler( OnDirectoryChangedDirComboBox );
				
				checkBox.CheckedChanged += new EventHandler( OnCheckCheckChanged );
			}
			
			public bool MultiSelect
			{
				set {
					multiSelect = value;
					mwfFileView.MultiSelect = value;
				}
				
				get {
					return multiSelect;
				}
			}
			
			public CheckBox CheckBox
			{
				set {
					checkBox = value;
				}
				
				get {
					return checkBox;
				}
			}
			
			void OnClickContextMenu( object sender, EventArgs e )
			{
				MenuItem senderMenuItem = sender as MenuItem;
				
				if ( senderMenuItem.Index == 0 )
				{
					senderMenuItem.Checked = !senderMenuItem.Checked;
					fileDialog.ShowHiddenFiles = senderMenuItem.Checked;
					mwfFileView.ShowHiddenFiles = fileDialog.ShowHiddenFiles;
					mwfFileView.UpdateFileView( currentDirectoryInfo_or_current_special_case );
				}
			}
			
			void OnClickOpenSaveButton( object sender, EventArgs e )
			{
				if ( !multiSelect )
				{
					if ( !show_special_case )
					{
						string fileFromComboBox = fileNameComboBox.Text.Trim( );
						
						if ( fileFromComboBox.Length > 0 ) 
						{
							if (!Path.IsPathRooted (fileFromComboBox))
								fileFromComboBox = Path.Combine( currentDirectoryName, fileFromComboBox );
							
							FileInfo fileInfo = new FileInfo (fileFromComboBox);
							if (fileInfo.Exists)
								currentFileName = fileFromComboBox;
							else 
							{
								DirectoryInfo dirInfo = new DirectoryInfo ( fileFromComboBox );
								if (dirInfo.Exists) 
								{
									ChangeDirectory( null, dirInfo.FullName );
									
									currentFileName = "";
									
									fileNameComboBox.Text = " ";
									return;
								}								
							}
						}
						else
							return;
					}
					else
					{
						if ( currentFileName == null || currentFileName == String.Empty )
						{
							currentFileName = fileNameComboBox.Text.Trim( );
							
							if ( currentFileName.Length > 0 )
							{
								FileInfo fileInfo = new FileInfo (currentFileName);
								if (!fileInfo.Exists)
								{
									DirectoryInfo dirInfo = new DirectoryInfo ( currentFileName );
									if (dirInfo.Exists) 
									{
										ChangeDirectory( null, dirInfo.FullName );
										
										currentFileName = "";
										
										fileNameComboBox.Text = " ";
										return;
									}								
								}
							}
							else
								return;
						}
						
						if ( currentDirectoryName == String.Empty )
							currentDirectoryName = Path.GetDirectoryName( currentFileName );
					}
					
					if ( fileDialog.fileDialogType == FileDialogType.OpenFileDialog )
					{
						if ( fileDialog.CheckFileExists )
						{
							if ( !File.Exists( currentFileName ) )
							{
								string message = "\"" + currentFileName + "\" doesn't exist. Please verify that you have entered the correct file name.";
								MessageBox.Show( message, fileDialog.OpenSaveButtonText, MessageBoxButtons.OK, MessageBoxIcon.Warning );
								
								currentFileName = "";
								
								return;
							}
						}
					}
					else // FileDialogType == SaveFileDialog
					{
						if ( fileDialog.OverwritePrompt )
						{
							if ( File.Exists( currentFileName ) )
							{
								string message = "\"" + currentFileName + "\" exists. Overwrite ?";
								DialogResult dr = MessageBox.Show( message, fileDialog.OpenSaveButtonText, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning );
								
								if ( dr == DialogResult.Cancel )
								{
									currentFileName = "";
									
									return;
								}
							}
						}
						
						if ( fileDialog.CreatePrompt )
						{
							if ( !File.Exists( currentFileName ) )
							{
								string message = "\"" + currentFileName + "\" doesn't exist. Create ?";
								DialogResult dr = MessageBox.Show( message, fileDialog.OpenSaveButtonText, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning );
								
								if ( dr == DialogResult.Cancel )
								{
									currentFileName = "";
									
									return;
								}
							}
						}
					}
					
					if ( fileDialog.fileDialogType == FileDialogType.SaveFileDialog )
					{
						if ( fileDialog.AddExtension && fileDialog.DefaultExt.Length > 0 )
						{
							if ( !currentFileName.EndsWith( fileDialog.DefaultExt ) )
							{
								currentFileName += "." + fileDialog.DefaultExt;
							}
						}
					}
					
					fileDialog.FileName = currentFileName;
					
					WriteRecentlyUsed();
				}
				else // multiSelect = true
				if ( fileDialog.fileDialogType != FileDialogType.SaveFileDialog )
				{
					if ( mwfFileView.SelectedItems.Count > 0 )
					{
						// first remove all selected directories
						ArrayList al = new ArrayList( );
						
						foreach ( ListViewItem lvi in mwfFileView.SelectedItems )
						{
							FileStruct fileStruct = (FileStruct)mwfFileView.FileHashtable[ lvi.Text ];
							
							if ( fileStruct.attributes != FileAttributes.Directory )
							{
								al.Add( fileStruct );
							}
						}
						
						fileDialog.FileName = ( (FileStruct)al[ 0 ] ).fullname;
						
						string[] filenames = new string[ al.Count ];
						
						for ( int i = 0; i < al.Count; i++ )
						{
							filenames[ i ] = ( (FileStruct)al[ i ] ).fullname;
						}
						
						fileDialog.SetFilenames( filenames );
					}
				}
				
				if ( fileDialog.CheckPathExists )
				{
					if ( !Directory.Exists( currentDirectoryName ) )
					{
						string message = "\"" + currentDirectoryName + "\" doesn't exist. Please verify that you have entered the correct directory name.";
						MessageBox.Show( message, fileDialog.OpenSaveButtonText, MessageBoxButtons.OK, MessageBoxIcon.Warning );
						
						if ( fileDialog.InitialDirectory == String.Empty )
							currentDirectoryName = Environment.CurrentDirectory;
						else
							currentDirectoryName = fileDialog.InitialDirectory;
						
						return;
					}
				}
				
				if ( fileDialog.RestoreDirectory )
					currentDirectoryName = restoreDirectory;
				
				CancelEventArgs cancelEventArgs = new CancelEventArgs( );
				
				cancelEventArgs.Cancel = false;
				
				fileDialog.OnFileOk( cancelEventArgs );
				
				fileDialog.form.Controls.Remove( this );
				fileDialog.form.DialogResult = DialogResult.OK;
			}
			
			void OnClickCancelButton( object sender, EventArgs e )
			{
				if ( fileDialog.RestoreDirectory )
					currentDirectoryName = restoreDirectory;
				
				CancelEventArgs cancelEventArgs = new CancelEventArgs( );
				
				cancelEventArgs.Cancel = true;
				
				fileDialog.OnFileOk( cancelEventArgs );
				
				fileDialog.form.Controls.Remove( this );
				fileDialog.form.DialogResult = DialogResult.Cancel;
			}
			
			void OnClickHelpButton( object sender, EventArgs e )
			{
				fileDialog.SendHelpRequest( e );
			}
			
			void OnClickSmallButtonToolBar( object sender, ToolBarButtonClickEventArgs e )
			{
				if ( e.Button == upToolBarButton )
				{
					if ( currentDirectoryInfo != null && currentDirectoryInfo.Parent != null )
						ChangeDirectory( null, currentDirectoryInfo.Parent.FullName );
				}
				else
				if ( e.Button == backToolBarButton )
				{
					PopDirectory( );
				}
				else
				if ( e.Button == newdirToolBarButton )
				{
					
				}
			}
			
			void OnClickMenuToolBarContextMenu( object sender, EventArgs e )
			{
				MenuItem senderMenuItem = (MenuItem)sender;
				
				previousCheckedMenuItem.Checked = false;
				senderMenuItem.Checked = true;
				previousCheckedMenuItem = senderMenuItem;
				
				// FIXME...
				
				switch ( senderMenuItem.Index  )
				{
					case 0:
						mwfFileView.View = View.SmallIcon;
						break;
					case 1:
						mwfFileView.View = View.LargeIcon;
						break;
					case 2:
						mwfFileView.View = View.LargeIcon;
						break;
					case 3:
						mwfFileView.View = View.List;
						break;
					case 4:
						mwfFileView.View = View.Details;
						break;
					default:
						break;
				}
				
				
				mwfFileView.UpdateFileView( currentDirectoryInfo_or_current_special_case );
			}
			
			void OnSelectedIndexChangedFileTypeComboBox( object sender, EventArgs e )
			{
				fileDialog.FilterIndex = fileTypeComboBox.SelectedIndex + 1;
				
				mwfFileView.FilterIndex = fileDialog.FilterIndex;
				
				mwfFileView.UpdateFileView( currentDirectoryInfo_or_current_special_case );
			}
			
			void OnSelectedFileChangedFileView( object sender, EventArgs e )
			{
				fileNameComboBox.Text = mwfFileView.FileName;
				currentFileName = mwfFileView.FullFileName;
			}
			
			void OnDirectoryChangedFileView( object sender, EventArgs e )
			{
				ChangeDirectory( sender, mwfFileView.FullFileName );
			}
			
			void OnForceDialogEndFileView( object sender, EventArgs e )
			{
				ForceDialogEnd( );
			}
			
			void OnSelectedFilesChangedFileView( object sender, EventArgs e )
			{
				fileNameComboBox.Text = mwfFileView.SelectedFilesString;
			}
			
			void OnDirectoryChangedDirComboBox( object sender, EventArgs e )
			{
				ChangeDirectory( sender, dirComboBox.CurrentPath );
			}
			
			void OnCheckCheckChanged( object sender, EventArgs e )
			{
				fileDialog.ReadOnlyChecked = checkBox.Checked;
			}
			
			public void UpdateFilters( )
			{
				ArrayList filters = fileDialog.FileFilter.FilterArrayList;
				
				fileTypeComboBox.Items.Clear( );
				
				fileTypeComboBox.BeginUpdate( );
				
				foreach ( FilterStruct fs in filters )
				{
					fileTypeComboBox.Items.Add( fs.filterName );
				}
				
				fileTypeComboBox.SelectedIndex = fileDialog.FilterIndex - 1;
				
				fileTypeComboBox.EndUpdate( );
				
				mwfFileView.FilterArrayList = filters;
				
				mwfFileView.FilterIndex = fileDialog.FilterIndex;
				
				mwfFileView.UpdateFileView( currentDirectoryInfo_or_current_special_case );
			}
			
			public void ChangeDirectory( object sender, string path_or_special_case )
			{
				show_special_case = false;
				
				if ( sender != dirComboBox )
					dirComboBox.CurrentPath = path_or_special_case;
				
				if ( sender != popupButtonPanel )
					popupButtonPanel.SetPopupButtonStateByPath( path_or_special_case );
				
				if ( currentDirectoryInfo != null )
					PushDirectory( currentDirectoryInfo );
				else
					PushDirectory( current_special_case );
				
				if ( path_or_special_case == recently_string )
				{
					currentDirectoryName = String.Empty;
					
					currentDirectoryInfo = null;
					show_special_case = true;
					
					current_special_case = recently_string;
					
					mwfFileView.UpdateFileView( recently_string );
				}
				else
				{
					currentDirectoryName = path_or_special_case;
					
					currentDirectoryInfo = new DirectoryInfo( path_or_special_case );
					
					mwfFileView.UpdateFileView( currentDirectoryInfo );
				}
			}
			
			public void ForceDialogEnd( )
			{
				OnClickOpenSaveButton( this, EventArgs.Empty );
			}
			
			private void PushDirectory( object directoryInfo_or_string )
			{
				directoryStack.Push( directoryInfo_or_string );
				backToolBarButton.Enabled = true;
			}
			
			private void PopDirectory( )
			{
				if ( directoryStack.Count == 0 )
					return;
				
				show_special_case = false;
				
				object directoryInfo_or_string = directoryStack.Pop( );
				
				if ( directoryInfo_or_string is DirectoryInfo )
				{
					currentDirectoryInfo = directoryInfo_or_string as DirectoryInfo;
				
					currentDirectoryName = currentDirectoryInfo.FullName;
					
					current_special_case = String.Empty;
				}
				else
				if ( directoryInfo_or_string is string )
				{
					currentDirectoryInfo = null;
					currentDirectoryName = String.Empty;
					show_special_case = true;
					current_special_case = directoryInfo_or_string as string;
				}
				
				if ( directoryStack.Count == 0 )
					backToolBarButton.Enabled = false;
				
				dirComboBox.CurrentPath = currentDirectoryName_or_special_case;
				
				popupButtonPanel.SetPopupButtonStateByPath( currentDirectoryName_or_special_case );
				
				mwfFileView.UpdateFileView( currentDirectoryInfo_or_current_special_case );
			}
			
			public void ResizeAndRelocateForHelpOrReadOnly( )
			{
				if ( fileDialog.ShowHelp || fileDialog.ShowReadOnly )
				{
					mwfFileView.Size = new Size( 449, 250 ); 
					fileNameLabel.Location = new Point( 102, 298 );
					fileNameComboBox.Location = new Point( 195, 298 );
					fileTypeLabel.Location = new Point( 102, 324 );
					fileTypeComboBox.Location = new Point( 195, 324 );
					openSaveButton.Location = new Point( 475, 298 );
					cancelButton.Location = new Point( 475, 324 );
				}
				else
				{
					mwfFileView.Size = new Size( 449, 282 );
					fileNameLabel.Location = new Point( 102, 330 );
					fileNameComboBox.Location = new Point( 195, 330 );
					fileTypeLabel.Location = new Point( 102, 356 );
					fileTypeComboBox.Location = new Point( 195, 356 );
					openSaveButton.Location = new Point( 475, 330 );
					cancelButton.Location = new Point( 475, 356 );
				}
				
				if ( fileDialog.ShowHelp )
					Controls.Add( helpButton );
				else
					Controls.Remove( helpButton );
				
				if ( fileDialog.ShowReadOnly )
					Controls.Add( checkBox );
				else
					Controls.Remove( checkBox );
			}
			
			private void WriteRecentlyUsed( )
			{
				int platform = (int) Environment.OSVersion.Platform;
				
				// on a *nix platform we use "$HOME/.recently-used" to store our recently used files (GNOME, libegg like)
				if ((platform == 4) || (platform == 128)) 
				{
					string personal_folder = ThemeEngine.Current.Places(UIIcon.PlacesPersonal);
					string recently_used_path = Path.Combine( personal_folder, ".recently-used" );
					
					if ( File.Exists( recently_used_path ) )
					{
						XmlDocument xml_doc = new XmlDocument( );
						xml_doc.Load( recently_used_path );
						
						XmlNode grand_parent_node = xml_doc.SelectSingleNode( "RecentFiles" );
						
						if ( grand_parent_node != null )
						{
							// create a new element
							XmlElement new_recent_item_node = xml_doc.CreateElement( "RecentItem" );
							
							XmlElement new_child = xml_doc.CreateElement( "URI" );
							UriBuilder ub = new UriBuilder( );
							ub.Path = currentFileName;
							ub.Host = null;
							ub.Scheme = "file";
							XmlText new_text_child = xml_doc.CreateTextNode( ub.ToString() );
							new_child.AppendChild( new_text_child );
							
							new_recent_item_node.AppendChild( new_child );
							
							new_child = xml_doc.CreateElement( "Mime-Type" );
							new_text_child = xml_doc.CreateTextNode( Mime.GetMimeTypeForFile( currentFileName ) );
							new_child.AppendChild( new_text_child );
							
							new_recent_item_node.AppendChild( new_child );
							
							new_child = xml_doc.CreateElement( "Timestamp" );
							long seconds = (long)( DateTime.UtcNow - new DateTime( 1970, 1, 1 ) ).TotalSeconds;
							new_text_child = xml_doc.CreateTextNode( seconds.ToString( ) );
							new_child.AppendChild( new_text_child );
							
							new_recent_item_node.AppendChild( new_child );
							
							new_child = xml_doc.CreateElement( "Groups" );
							
							new_recent_item_node.AppendChild( new_child );
							
							// now search the nodes in grand_parent_node for another instance of the new uri and if found remove it
							// so that the new node is the first one
							foreach ( XmlNode n in grand_parent_node.ChildNodes )
							{
								XmlNode uri_node = n.SelectSingleNode( "URI" );
								if ( uri_node != null )
								{
									XmlNode uri_node_child = uri_node.FirstChild;
									if ( uri_node_child is XmlText )
										if ( ub.ToString() == ((XmlText)uri_node_child).Data )
										{
											grand_parent_node.RemoveChild( n );
											break;
										}
								}
							}
							
							// prepend the new recent item to the grand parent node list
							grand_parent_node.PrependChild( new_recent_item_node );
							
							// limit the # of RecentItems to 10
							if ( grand_parent_node.ChildNodes.Count > 10 )
							{
								while( grand_parent_node.ChildNodes.Count > 10 )
									grand_parent_node.RemoveChild( grand_parent_node.LastChild );
							}
							
							try {
								xml_doc.Save( recently_used_path );
							} catch ( Exception e ) {
							}
						}
					}
					else // create a new .recently-used file
					{
						XmlDocument xml_doc = new XmlDocument();
						xml_doc.AppendChild( xml_doc.CreateXmlDeclaration( "1.0", "", "" ) );
						
						XmlElement recentFiles_element = xml_doc.CreateElement( "RecentFiles" );
						
						XmlElement new_recent_item_node = xml_doc.CreateElement( "RecentItem" );
						
						XmlElement new_child = xml_doc.CreateElement( "URI" );
						UriBuilder ub = new UriBuilder( );
						ub.Path = currentFileName;
						ub.Host = null;
						ub.Scheme = "file";
						XmlText new_text_child = xml_doc.CreateTextNode( ub.ToString() );
						new_child.AppendChild( new_text_child );
						
						new_recent_item_node.AppendChild( new_child );
						
						new_child = xml_doc.CreateElement( "Mime-Type" );
						new_text_child = xml_doc.CreateTextNode( Mime.GetMimeTypeForFile( currentFileName ) );
						new_child.AppendChild( new_text_child );
						
						new_recent_item_node.AppendChild( new_child );
						
						new_child = xml_doc.CreateElement( "Timestamp" );
						long seconds = (long)( DateTime.UtcNow - new DateTime( 1970, 1, 1 ) ).TotalSeconds;
						new_text_child = xml_doc.CreateTextNode( seconds.ToString( ) );
						new_child.AppendChild( new_text_child );
						
						new_recent_item_node.AppendChild( new_child );
						
						new_child = xml_doc.CreateElement( "Groups" );
						
						new_recent_item_node.AppendChild( new_child );
						
						recentFiles_element.AppendChild( new_recent_item_node );
						
						xml_doc.AppendChild( recentFiles_element );

						try {
							xml_doc.Save( recently_used_path );
						} catch ( Exception e ) {
						}
					}
				}
			}
			
			private object currentDirectoryInfo_or_current_special_case
			{
				get {
					if ( currentDirectoryInfo != null )
						return currentDirectoryInfo;
					else
						return current_special_case;
				}
			}
			
			private string currentDirectoryName_or_special_case
			{
				get {
					if ( currentDirectoryName != String.Empty )
						return currentDirectoryName;
					else
						return current_special_case;
				}
			}
			
			internal class PopupButtonPanel : Panel
			{
				internal class PopupButton : Control
				{
					internal enum PopupButtonState
					{ Normal, Down, Up}
					
					private Image image = null;
					private PopupButtonState popupButtonState = PopupButtonState.Normal;
					private StringFormat text_format = new StringFormat();
					
					public PopupButton( )
					{
						text_format.Alignment = StringAlignment.Center;
						text_format.LineAlignment = StringAlignment.Far;
						
						SetStyle( ControlStyles.DoubleBuffer, true );
						SetStyle( ControlStyles.AllPaintingInWmPaint, true );
						SetStyle( ControlStyles.UserPaint, true );
					}
					
					public Image Image
					{
						set {
							image = value;
							Refresh( );
						}
						
						get {
							return image;
						}
					}
					
					public PopupButtonState ButtonState
					{
						set {
							popupButtonState = value;
							Refresh( );
						}
						
						get {
							return popupButtonState;
						}
					}
					
					protected override void OnPaint( PaintEventArgs pe )
					{
						Draw( pe );
						
						base.OnPaint( pe );
					}
					
					private void Draw( PaintEventArgs pe )
					{
						Graphics gr = pe.Graphics;
						
						gr.FillRectangle( ThemeEngine.Current.ResPool.GetSolidBrush( BackColor ), ClientRectangle );
						
						// draw image
						if ( image != null )
						{
							int i_x = ( ClientSize.Width - image.Width ) / 2;
							int i_y = 4;
							gr.DrawImage( image, i_x, i_y );
						}
						
						if ( Text != String.Empty )
						{
							Rectangle text_rect = Rectangle.Inflate( ClientRectangle, -4, -4 );
							
							gr.DrawString( Text, Font, ThemeEngine.Current.ResPool.GetSolidBrush( ForeColor ), text_rect, text_format );
						}
						
						switch ( popupButtonState )
						{
							case PopupButtonState.Up:
								gr.DrawLine( ThemeEngine.Current.ResPool.GetPen( Color.White ), 0, 0, ClientSize.Width - 1, 0 );
								gr.DrawLine( ThemeEngine.Current.ResPool.GetPen( Color.White ), 0, 0, 0, ClientSize.Height - 1 );
								gr.DrawLine( ThemeEngine.Current.ResPool.GetPen( Color.Black ), ClientSize.Width - 1, 0, ClientSize.Width - 1, ClientSize.Height - 1 );
								gr.DrawLine( ThemeEngine.Current.ResPool.GetPen( Color.Black ), 0, ClientSize.Height - 1, ClientSize.Width - 1, ClientSize.Height - 1 );
								break;
								
							case PopupButtonState.Down:
								gr.DrawLine( ThemeEngine.Current.ResPool.GetPen( Color.Black ), 0, 0, ClientSize.Width - 1, 0 );
								gr.DrawLine( ThemeEngine.Current.ResPool.GetPen( Color.Black ), 0, 0, 0, ClientSize.Height - 1 );
								gr.DrawLine( ThemeEngine.Current.ResPool.GetPen( Color.White ), ClientSize.Width - 1, 0, ClientSize.Width - 1, ClientSize.Height - 1 );
								gr.DrawLine( ThemeEngine.Current.ResPool.GetPen( Color.White ), 0, ClientSize.Height - 1, ClientSize.Width - 1, ClientSize.Height - 1 );
								break;
						}
					}
					
					protected override void OnMouseEnter( EventArgs e )
					{
						if ( popupButtonState != PopupButtonState.Down )
							popupButtonState = PopupButtonState.Up;
						Refresh( );
						base.OnMouseEnter( e );
					}
					
					protected override void OnMouseLeave( EventArgs e )
					{
						if ( popupButtonState != PopupButtonState.Down )
							popupButtonState = PopupButtonState.Normal;
						Refresh( );
						base.OnMouseLeave( e );
					}
					
					protected override void OnClick( EventArgs e )
					{
						popupButtonState = PopupButtonState.Down;
						Refresh( );
						base.OnClick( e );
					}
				}
				
				private FileDialogPanel fileDialogPanel;
				
				private PopupButton lastOpenButton;
				private PopupButton desktopButton;
				private PopupButton homeButton;
				private PopupButton workplaceButton;
				private PopupButton networkButton;
				
				private PopupButton lastPopupButton = null;
				
				private int platform = (int) Environment.OSVersion.Platform;
				
				public PopupButtonPanel( FileDialogPanel fileDialogPanel )
				{
					this.fileDialogPanel = fileDialogPanel;
					
					BorderStyle = BorderStyle.Fixed3D;
					BackColor = Color.FromArgb( 128, 128, 128 );
					Size = new Size( 85, 336 );
					
					lastOpenButton = new PopupButton( );
					desktopButton = new PopupButton( );
					homeButton = new PopupButton( );
					workplaceButton = new PopupButton( );
					networkButton = new PopupButton( );
					
					lastOpenButton.Size = new Size( 82, 64 );
					lastOpenButton.Image = ThemeEngine.Current.Images(UIIcon.PlacesRecentDocuments, 38);
					lastOpenButton.BackColor = BackColor;
					lastOpenButton.ForeColor = Color.White;
					lastOpenButton.Location = new Point( 2, 2 );
					lastOpenButton.Text = "Last Open";
					lastOpenButton.Click += new EventHandler( OnClickButton );
					
					desktopButton.Image = ThemeEngine.Current.Images(UIIcon.PlacesDesktop, 38);
					desktopButton.BackColor = BackColor;
					desktopButton.ForeColor = Color.White;
					desktopButton.Size = new Size( 82, 64 );
					desktopButton.Location = new Point( 2, 66 );
					desktopButton.Text = "Desktop";
					desktopButton.Click += new EventHandler( OnClickButton );
					
					homeButton.Image = ThemeEngine.Current.Images(UIIcon.PlacesPersonal, 38);
					homeButton.BackColor = BackColor;
					homeButton.ForeColor = Color.White;
					homeButton.Size = new Size( 82, 64 );
					homeButton.Location = new Point( 2, 130 );
					homeButton.Text = "Home";
					homeButton.Click += new EventHandler( OnClickButton );
					
					workplaceButton.Image = ThemeEngine.Current.Images(UIIcon.PlacesMyComputer, 38);
					workplaceButton.BackColor = BackColor;
					workplaceButton.ForeColor = Color.White;
					workplaceButton.Size = new Size( 82, 64 );
					workplaceButton.Location = new Point( 2, 194 );
					workplaceButton.Text = "Workplace";
					workplaceButton.Click += new EventHandler( OnClickButton );
					
					networkButton.Image = ThemeEngine.Current.Images(UIIcon.PlacesMyNetwork, 38);
					networkButton.BackColor = BackColor;
					networkButton.ForeColor = Color.White;
					networkButton.Size = new Size( 82, 64 );
					networkButton.Location = new Point( 2, 258 );
					networkButton.Text = "Network";
					networkButton.Click += new EventHandler( OnClickButton );
					
					Controls.Add( lastOpenButton );
					Controls.Add( desktopButton );
					Controls.Add( homeButton );
					Controls.Add( workplaceButton );
					Controls.Add( networkButton );
				}
				
				void OnClickButton( object sender, EventArgs e )
				{
					if ( lastPopupButton != null && (PopupButton)sender != lastPopupButton )
						lastPopupButton.ButtonState = PopupButton.PopupButtonState.Normal;
					lastPopupButton = sender as PopupButton;
					
					if ( sender == lastOpenButton )
					{
						if ((platform == 4) || (platform == 128))
							// do NOT change the following line!
							// FileDialog uses a special handling for recently used files on *nix
							// recently used files are not stored as links in a directory but
							// as a xml file called .recently-used in the users home dir
							// This matches the Freedesktop.org spec which gnome uses
							fileDialogPanel.ChangeDirectory( this, FileDialog.FileDialogPanel.recently_string );
						else
							fileDialogPanel.ChangeDirectory(this, ThemeEngine.Current.Places(UIIcon.PlacesRecentDocuments));
					}
					else
					if ( sender == desktopButton )
					{
						fileDialogPanel.ChangeDirectory(this, ThemeEngine.Current.Places(UIIcon.PlacesDesktop));
					}
					else
					if ( sender == homeButton )
					{
						fileDialogPanel.ChangeDirectory(this, ThemeEngine.Current.Places(UIIcon.PlacesPersonal));
					}
					else
					if ( sender == workplaceButton )
					{
						if ((platform == 4) || (platform == 128))
							// do NOT change the following line!
							// on *nix we do not have a special folder MyComputer
							// so we use the root dir
							// FIXME: the output should be the same as in gnome's Places->Computer
							fileDialogPanel.ChangeDirectory(this, "/" );
						else
							fileDialogPanel.ChangeDirectory(this, ThemeEngine.Current.Places(UIIcon.PlacesMyComputer));
					}
					else
					if ( sender == networkButton )
					{
						// FIXME: only available on win, see Theme.cs, MonoTodo
						fileDialogPanel.ChangeDirectory(this, ThemeEngine.Current.Places(UIIcon.PlacesMyNetwork));
					}
				}
				
				public void SetPopupButtonStateByPath( string path )
				{
					if ( path == FileDialog.FileDialogPanel.recently_string || 
					    path == ThemeEngine.Current.Places(UIIcon.PlacesRecentDocuments) )
					{
						if ( lastPopupButton != lastOpenButton )
						{
							if ( lastPopupButton != null )
								lastPopupButton.ButtonState = PopupButton.PopupButtonState.Normal;
							lastOpenButton.ButtonState = PopupButton.PopupButtonState.Down;
							lastPopupButton = lastOpenButton;
						}
					}
					else
					if ( path == ThemeEngine.Current.Places(UIIcon.PlacesDesktop) )
					{
						if ( lastPopupButton != desktopButton )
						{
							if ( lastPopupButton != null )
								lastPopupButton.ButtonState = PopupButton.PopupButtonState.Normal;
							desktopButton.ButtonState = PopupButton.PopupButtonState.Down;
							lastPopupButton = desktopButton;
						}
					}
					else
					if ( path == ThemeEngine.Current.Places(UIIcon.PlacesPersonal) )
					{
						if ( lastPopupButton != homeButton )
						{
							if ( lastPopupButton != null )
								lastPopupButton.ButtonState = PopupButton.PopupButtonState.Normal;
							homeButton.ButtonState = PopupButton.PopupButtonState.Down;
							lastPopupButton = homeButton;
						}
					}
					else
					if ( path == "/" || 
					    path == ThemeEngine.Current.Places(UIIcon.PlacesMyComputer) )
					{
						if ( lastPopupButton != workplaceButton )
						{
							if ( lastPopupButton != null )
								lastPopupButton.ButtonState = PopupButton.PopupButtonState.Normal;
							workplaceButton.ButtonState = PopupButton.PopupButtonState.Down;
							lastPopupButton = workplaceButton;
						}
					}
					// TODO: add networkPopupButton
					else
					{
						if ( lastPopupButton != null )
						{
							lastPopupButton.ButtonState = PopupButton.PopupButtonState.Normal;
							lastPopupButton = null;
						}
					}
				}
			}
		}
	}
	
	internal struct FilterStruct
	{
		public string filterName;
		public StringCollection filters;
		
		public FilterStruct( string filterName, string filter )
		{
			this.filterName = filterName;
			
			filters =  new StringCollection( );
			
			SplitFilters( filter );
		}
		
		private void SplitFilters( string filter )
		{
			string[] split = filter.Split( new Char[] {';'} );
			
			filters.AddRange( split );
		}
	}
	
	internal struct FileStruct
	{
		public FileStruct( string fullname, FileAttributes attributes, long size )
		{
			this.fullname = fullname;
			this.attributes = attributes;
			this.size = size;
		}
		
		public string fullname;
		public FileAttributes attributes;
		public long size;
	}
	
	// MWFFileView
	internal class MWFFileView : ListView
	{
		private ArrayList filterArrayList;
		
		// store the FileStruct of all files in the current directory
		private Hashtable fileHashtable = new Hashtable();
		
		private bool showHiddenFiles = false;
		
		private EventHandler on_selected_file_changed;
		private EventHandler on_selected_files_changed;
		private EventHandler on_directory_changed;
		private EventHandler on_force_dialog_end;
		
		private string fileName;
		private string fullFileName;
		private string selectedFilesString;
		
		private int filterIndex;
		
		private ToolTip toolTip;
		private int oldItemIndexForToolTip = -1;
		
		public MWFFileView( )
		{
			toolTip = new ToolTip ();
			toolTip.InitialDelay = 300;
			toolTip.ReshowDelay = 0; 
			
			LabelWrap = true;
			
			SmallImageList = MimeIconEngine.SmallIcons;
			LargeImageList = MimeIconEngine.LargeIcons;
			
			View = View.List;
		}
		
		public ArrayList FilterArrayList
		{
			set {
				filterArrayList = value;
			}
			
			get {
				return filterArrayList;
			}
		}
		
		public Hashtable FileHashtable
		{
			set {
				fileHashtable = value;
			}
			
			get {
				return fileHashtable;
			}
		}
		
		public bool ShowHiddenFiles
		{
			set {
				showHiddenFiles = value;
			}
			
			get {
				return showHiddenFiles;
			}
		}
		
		public string FileName
		{
			set {
				fileName = value;
			}
			
			get {
				return fileName;
			}
		}
		
		public string FullFileName
		{
			set {
				fullFileName = value;
			}
			
			get {
				return fullFileName;
			}
		}
		
		public int FilterIndex
		{
			set {
				filterIndex = value;
			}
			
			get {
				return filterIndex;
			}
		}
		
		public string SelectedFilesString
		{
			set {
				selectedFilesString = value;
			}
			
			get {
				return selectedFilesString;
			}
		}
		
		private ArrayList GetFileInfoArrayList( DirectoryInfo directoryInfo )
		{
			ArrayList arrayList = new ArrayList( );
			
			if ( filterArrayList != null && filterArrayList.Count != 0 )
			{
				FilterStruct fs = (FilterStruct)filterArrayList[ filterIndex - 1 ];
				
				foreach ( string s in fs.filters )
					arrayList.AddRange( directoryInfo.GetFiles( s ) );
			}
			else
				arrayList.AddRange( directoryInfo.GetFiles( ) );
			
			return arrayList;
		}
		
		private ArrayList GetFileInfoArrayListByArrayList( ArrayList al )
		{
			ArrayList arrayList = new ArrayList( );
			
			foreach ( string path in al )
			{
				if ( File.Exists( path ) )
				{
					FileInfo fi = new FileInfo( path );
					arrayList.Add( fi );
				}
			}
			
			return arrayList;
		}
		
		public void UpdateFileView( object directoryInfo_or_string )
		{
			if ( directoryInfo_or_string is DirectoryInfo )
				UpdateFileViewByDirectoryInfo( directoryInfo_or_string as DirectoryInfo);
			else
				UpdateFileViewByString( directoryInfo_or_string as string );
		}

		private void UpdateFileViewByDirectoryInfo( DirectoryInfo inputDirectoryInfo ) 
		{
			DirectoryInfo directoryInfo = inputDirectoryInfo;
			
			DirectoryInfo[] directoryInfoArray = directoryInfo.GetDirectories( );
			
			ArrayList fileInfoArrayList = GetFileInfoArrayList( directoryInfo );
			
			fileHashtable.Clear( );
			
			BeginUpdate( );
			
			Items.Clear( );
			SelectedItems.Clear( );
			
			foreach ( DirectoryInfo directoryInfoi in directoryInfoArray ) {
				if ( !ShowHiddenFiles )
					if ( directoryInfoi.Name.StartsWith( "." ) || directoryInfoi.Attributes == FileAttributes.Hidden )
						continue;
				
				FileStruct fileStruct = new FileStruct( );
				
				fileStruct.fullname = directoryInfoi.FullName;
				
				ListViewItem listViewItem = new ListViewItem( directoryInfoi.Name );
				
				int index = MimeIconEngine.GetIconIndexForMimeType( "inode/directory" );
				
				listViewItem.ImageIndex = index;
				
				listViewItem.SubItems.Add( "" );
				listViewItem.SubItems.Add( "Directory" );
				listViewItem.SubItems.Add( directoryInfoi.LastAccessTime.ToShortDateString( ) + " " + directoryInfoi.LastAccessTime.ToShortTimeString( ) );
				
				fileStruct.attributes = FileAttributes.Directory;
				
				fileHashtable.Add( directoryInfoi.Name, fileStruct );
				
				Items.Add( listViewItem );
			}
			
			foreach ( FileInfo fileInfo in fileInfoArrayList ) {
				DoOneFileInfo( fileInfo );
			}
			
			EndUpdate( );
		}
		
		private void UpdateFileViewByString( string kind ) 
		{
			if ( kind == FileDialog.FileDialogPanel.recently_string )
			{
				ArrayList fileInfoArrayList = GetFileInfoArrayListByArrayList( GetFreedesktopSpecRecentlyUsed() );
				
				fileHashtable.Clear( );
				
				BeginUpdate( );
				
				Items.Clear( );
				SelectedItems.Clear( );
				
				foreach ( FileInfo fileInfo in fileInfoArrayList )
				{
					DoOneFileInfo(fileInfo);
				}
				
				EndUpdate( );
			}
		}

		private void DoOneFileInfo( FileInfo fileInfo ) 
		{
			if ( !ShowHiddenFiles )
				if ( fileInfo.Name.StartsWith( "." )  || fileInfo.Attributes == FileAttributes.Hidden )
					return;
			
			FileStruct fileStruct = new FileStruct( );
			
			fileStruct.fullname = fileInfo.FullName;
			
			string fileName = fileInfo.Name;

			if (fileHashtable.ContainsKey (fileName)) {
				int i = 1;
				while(fileHashtable.ContainsKey (fileName + "[" + i + "]")) {
					i++;
				}
				fileName += "[" + i + "]";
			}
			
			ListViewItem listViewItem = new ListViewItem( fileName );
			
			listViewItem.ImageIndex = MimeIconEngine.GetIconIndexForFile( fileStruct.fullname );
			
			long fileLen = 1;
			try {
				if ( fileInfo.Length > 1024 )
					fileLen = fileInfo.Length / 1024;
			} catch ( Exception e ) {
				fileLen = 1;
			}
			
			fileStruct.size = fileLen;
			
			listViewItem.SubItems.Add( fileLen.ToString( ) + " KB" );
			listViewItem.SubItems.Add( "File" );
			listViewItem.SubItems.Add( fileInfo.LastAccessTime.ToShortDateString( ) + " " + fileInfo.LastAccessTime.ToShortTimeString( ) );
			
			fileStruct.attributes = FileAttributes.Normal;
			
			fileHashtable.Add( fileName, fileStruct );
			
			Items.Add( listViewItem );
		}
		
		private ArrayList GetFreedesktopSpecRecentlyUsed( ) 
		{
			// check for GNOME and KDE
			string personal_folder = ThemeEngine.Current.Places(UIIcon.PlacesPersonal);
			string recently_used_path = Path.Combine( personal_folder, ".recently-used" );
			
			ArrayList files_al = new ArrayList( );
			
			// GNOME
			if ( File.Exists( recently_used_path ) )
			{
				try
				{
					XmlTextReader xtr = new XmlTextReader( recently_used_path );
					while ( xtr.Read( ) ) 
					{
						if ( xtr.NodeType == XmlNodeType.Element && xtr.Name.ToUpper() == "URI" )
						{
							xtr.Read();
							Uri uri = new Uri( xtr.Value );
							if ( !files_al.Contains( uri.LocalPath ) )
								files_al.Add( uri.LocalPath );
						}
					}
					xtr.Close();
				} catch ( Exception e )
				{
					
				}
			}
			
			// KDE
			string full_kde_recent_document_dir = personal_folder
				+ "/"
				+ ".kde/share/apps/RecentDocuments";
			
			if ( Directory.Exists( full_kde_recent_document_dir ) )
			{
				string[] files = Directory.GetFiles( full_kde_recent_document_dir, "*.desktop" );
				
				foreach( string file_name in files )
				{
					StreamReader sr = new StreamReader( file_name );
					
					string line = sr.ReadLine();
					
					while (line != null) 
					{
						line = line.Trim();
						
						if ( line.StartsWith( "URL=" ) )
						{
							line = line.Replace( "URL=", "" );
							line = line.Replace( "$HOME", personal_folder );
							
							Uri uri = new Uri( line );
							if ( !files_al.Contains( uri.LocalPath ) )
								files_al.Add( uri.LocalPath );
							break;
						}
						
						line = sr.ReadLine();
					}
					
					sr.Close();
				}
			}
			
			
			return files_al;
		}
		
		protected override void OnClick( EventArgs e )
		{
			if ( !MultiSelect )
			{
				if ( SelectedItems.Count > 0 )
				{
					ListViewItem listViewItem = SelectedItems[ 0 ];
					
					FileStruct fileStruct = (FileStruct)fileHashtable[ listViewItem.Text ];
					
					if ( fileStruct.attributes != FileAttributes.Directory )
					{
						fileName = listViewItem.Text;
						fullFileName = fileStruct.fullname;
						
						if ( on_selected_file_changed != null )
							on_selected_file_changed( this, EventArgs.Empty );
					}
				}
			}
			
			base.OnClick( e );
		}
		
		protected override void OnDoubleClick( EventArgs e )
		{
			if ( SelectedItems.Count > 0 )
			{
				ListViewItem listViewItem = SelectedItems[ 0 ];
				
				FileStruct fileStruct = (FileStruct)fileHashtable[ listViewItem.Text ];
				
				if ( fileStruct.attributes == FileAttributes.Directory )
				{
					fullFileName = fileStruct.fullname;
					
					if ( on_directory_changed != null )
						on_directory_changed( this, EventArgs.Empty );
				}
				else
				{
					fileName = listViewItem.Text;
					fullFileName = fileStruct.fullname;
					
					if ( on_selected_file_changed != null )
						on_selected_file_changed( this, EventArgs.Empty );
					
					if ( on_force_dialog_end != null )
						on_force_dialog_end( this, EventArgs.Empty );
					
					return;
				}
			}
			
			base.OnDoubleClick( e );
		}
		
		protected override void OnSelectedIndexChanged( EventArgs e )
		{
			if ( MultiSelect )
			{
				if ( SelectedItems.Count > 0 )
				{
					selectedFilesString = "";
					
					if ( SelectedItems.Count == 1 )
					{
						FileStruct fileStruct = (FileStruct)fileHashtable[ SelectedItems[ 0 ].Text ];
						
						if ( fileStruct.attributes != FileAttributes.Directory )
							selectedFilesString = SelectedItems[ 0 ].Text;
					}
					else
					{
						foreach ( ListViewItem lvi in SelectedItems )
						{
							FileStruct fileStruct = (FileStruct)fileHashtable[ lvi.Text ];
							
							if ( fileStruct.attributes != FileAttributes.Directory )
								selectedFilesString += "\"" + lvi.Text + "\" ";
						}
					}
					
					if ( on_selected_files_changed != null )
						on_selected_files_changed( this, EventArgs.Empty );
				}
			}
			
			base.OnSelectedIndexChanged( e );
		}
		
		protected override void OnMouseMove (MouseEventArgs e)
		{
			ListViewItem item = GetItemAt (e.X, e.Y);
			
			if (item != null) {
				int currentItemIndex = item.Index;
				
				if (currentItemIndex != oldItemIndexForToolTip) {
					oldItemIndexForToolTip = currentItemIndex;
					
					if (toolTip != null && toolTip.Active)
						toolTip.Active = false;
					
					FileStruct fileStruct = (FileStruct)fileHashtable [item.Text];
					
					string output = String.Empty;
					
					if (fileStruct.attributes != FileAttributes.Directory) {
						output = String.Format ("File: {0}", fileStruct.fullname);
					}
					else
						output = String.Format ("Directory: {0}\n", fileStruct.fullname);
					
					toolTip.SetToolTip (this, output);	
					
					toolTip.Active = true;
				}
			}
			
			base.OnMouseMove (e);
		}
		
		public event EventHandler SelectedFileChanged
		{
			add { on_selected_file_changed += value; }
			remove { on_selected_file_changed -= value; }
		}
		
		public event EventHandler SelectedFilesChanged
		{
			add { on_selected_files_changed += value; }
			remove { on_selected_files_changed -= value; }
		}
		
		public event EventHandler DirectoryChanged
		{
			add { on_directory_changed += value; }
			remove { on_directory_changed -= value; }
		}
		
		public event EventHandler ForceDialogEnd
		{
			add { on_force_dialog_end += value; }
			remove { on_force_dialog_end -= value; }
		}
	}
	
	internal class FileFilter
	{
		private ArrayList filterArrayList = new ArrayList();
		
		private string filter;
		
		public FileFilter( )
		{}
		
		public FileFilter( string filter )
		{
			this.filter = filter;
			
			SplitFilter( );
		}
		
		public ArrayList FilterArrayList
		{
			set {
				filterArrayList = value;
			}
			
			get {
				return filterArrayList;
			}
		}
		
		public string Filter
		{
			set {
				filter = value;
				
				SplitFilter( );
			}
			
			get {
				return filter;
			}
		}
		
		private void SplitFilter( )
		{
			filterArrayList.Clear( );
			
			if ( filter == null )
				throw new NullReferenceException( "Filter" );
			
			if ( filter.Length == 0 )
				return;
			
			string[] filters = filter.Split( new Char[] {'|'} );
			
			if ( ( filters.Length % 2 ) != 0 )
				throw new ArgumentException( "Filter" );
			
			for ( int i = 0; i < filters.Length; i += 2 )
			{
				FilterStruct filterStruct = new FilterStruct( filters[ i ], filters[ i + 1 ] );
				
				filterArrayList.Add( filterStruct );
			}
		}
	}
	
	internal class DirComboBox : ComboBox
	{
		internal class DirComboBoxItem
		{
			private int imageIndex;
			private string name;
			private string path;
			private int xPos;
			
			public DirComboBoxItem( int imageIndex, string name, string path, int xPos )
			{
				this.imageIndex = imageIndex;
				this.name = name;
				this.path = path;
				this.XPos = xPos;
			}
			
			public int ImageIndex
			{
				set {
					imageIndex = value;
				}
				
				get {
					return imageIndex;
				}
			}
			
			public string Name
			{
				set {
					name = value;
				}
				
				get {
					return name;
				}
			}
			
			public string Path
			{
				set {
					path = value;
				}
				
				get {
					return path;
				}
			}
			
			public int XPos
			{
				set {
					xPos = value;
				}
				
				get {
					return xPos;
				}
			}
		}
		
		private ImageList imageList = new ImageList();
		
		private string currentPath;
		
		private EventHandler on_directory_changed;
		
		private bool currentpath_internal_change = false;
		
		private int platform = (int) Environment.OSVersion.Platform;
		private string recently_tmp;
		private string workplace_tmp;
		private Stack dirStack = new Stack();
		
		public DirComboBox( )
		{
			DrawMode = DrawMode.OwnerDrawFixed;
			
			imageList.ColorDepth = ColorDepth.Depth32Bit;
			imageList.ImageSize = new Size( 16, 16 );
			imageList.Images.Add(ThemeEngine.Current.Images(UIIcon.PlacesRecentDocuments, 16));
			imageList.Images.Add(ThemeEngine.Current.Images(UIIcon.PlacesDesktop, 16));
			imageList.Images.Add(ThemeEngine.Current.Images(UIIcon.PlacesPersonal, 16));
			imageList.Images.Add(ThemeEngine.Current.Images(UIIcon.PlacesMyComputer, 16));
			imageList.Images.Add(ThemeEngine.Current.Images(UIIcon.PlacesMyNetwork, 16));
			imageList.Images.Add(ThemeEngine.Current.Images(UIIcon.NormalFolder, 16));
			imageList.TransparentColor = Color.Transparent;
			
			if ((platform == 4) || (platform == 128))
			{
				recently_tmp = FileDialog.FileDialogPanel.recently_string;
				workplace_tmp = "/";
			}
			else
			{
				recently_tmp = ThemeEngine.Current.Places(UIIcon.PlacesRecentDocuments);
				workplace_tmp = ThemeEngine.Current.Places(UIIcon.PlacesMyComputer);
			}
			
			Items.AddRange( new object[] {
						new DirComboBoxItem( 0, "Recently used", recently_tmp, 0 ),
						new DirComboBoxItem( 1, "Desktop", ThemeEngine.Current.Places(UIIcon.PlacesDesktop), 0),
						new DirComboBoxItem( 2, "Home", ThemeEngine.Current.Places(UIIcon.PlacesPersonal), 0 ),
						new DirComboBoxItem( 3, "Workplace", workplace_tmp, 0 )
				       }
				       );
		}
		
		public string CurrentPath
		{
			set {
				currentPath = value;
				
				currentpath_internal_change = true;
				
				CreateComboList( );
			}
			get {
				return currentPath;
			}
		}
		
		private void CreateComboList( )
		{
			int selection = -1;
			int child_of = - 1;
			
			if ( currentPath == recently_tmp ||
			    currentPath == ThemeEngine.Current.Places(UIIcon.PlacesDesktop) ||
			    currentPath == ThemeEngine.Current.Places(UIIcon.PlacesPersonal) || 
			    currentPath == workplace_tmp )
			{
				if ( currentPath == recently_tmp )
					selection = 0;
				else
				if ( currentPath == ThemeEngine.Current.Places(UIIcon.PlacesDesktop) )
					selection = 1;
				else
				if ( currentPath == ThemeEngine.Current.Places(UIIcon.PlacesPersonal) )
					selection = 2;
				else
				if( currentPath == workplace_tmp )
					selection = 3;
			}
			else
				child_of = CheckChildOf();
			
			
			BeginUpdate( );
			
			Items.Clear( );
			
			Items.Add( new DirComboBoxItem( 0, "Recently used", recently_tmp, 0 ) );
			
			Items.Add( new DirComboBoxItem( 1, "Desktop", ThemeEngine.Current.Places(UIIcon.PlacesDesktop), 0 ) );
			if ( child_of == 1 )
				selection = AppendToParent();
			
			Items.Add( new DirComboBoxItem( 2, "Home", ThemeEngine.Current.Places(UIIcon.PlacesPersonal), 0 ) );
			if ( child_of == 2 )
				selection = AppendToParent();
				
			Items.Add( new DirComboBoxItem( 3, "Workplace", workplace_tmp, 0 ) );
			if ( child_of == 3 )
				selection = AppendToParent();
				
			if ( selection != -1 )
				SelectedIndex = selection;

			EndUpdate( );
		}
		
		private int CheckChildOf()
		{
			dirStack.Clear();
			DirectoryInfo di = new DirectoryInfo( currentPath );
			
			dirStack.Push( di );
			
			while ( di.Parent != null )
			{
				di = di.Parent;
				if ( di.FullName == ThemeEngine.Current.Places(UIIcon.PlacesDesktop) )
					return 1;
				else
				if ( di.FullName == ThemeEngine.Current.Places(UIIcon.PlacesPersonal) )
					return 2;
				else
				if ( di.FullName == workplace_tmp )
					return 3;
				
				dirStack.Push( di );
			}
			
			return -1;
		}
		
		private int AppendToParent()
		{
			int xPos = 0;
			int selection = -1;
			
			while ( dirStack.Count != 0 )
			{
				DirectoryInfo dii = dirStack.Pop( ) as DirectoryInfo;
				selection = Items.Add( new DirComboBoxItem( 5, dii.Name, dii.FullName, xPos + 4 ) );
				xPos += 4;
			}
			
			return selection;
		}
		
		protected override void OnDrawItem( DrawItemEventArgs e )
		{
			if ( e.Index == -1 )
				return;
			
			Bitmap bmp = new Bitmap( e.Bounds.Width, e.Bounds.Height, e.Graphics );
			Graphics gr = Graphics.FromImage( bmp );
			
			DirComboBoxItem dcbi = Items[ e.Index ] as DirComboBoxItem;
			
			Color backColor = e.BackColor;
			Color foreColor = e.ForeColor;
			
			int xPos = dcbi.XPos;
			
			if ( ( e.State & DrawItemState.ComboBoxEdit ) != 0 )
				xPos = 0;
			else
			if ( ( e.State & DrawItemState.Selected ) == DrawItemState.Selected )
			{
				backColor = ThemeEngine.Current.ColorHighlight;
				foreColor = ThemeEngine.Current.ColorHighlightText;
			}
			
			gr.FillRectangle( ThemeEngine.Current.ResPool.GetSolidBrush( backColor ), new Rectangle( 0, 0, bmp.Width, bmp.Height ) );
			
			gr.DrawString( dcbi.Name, e.Font , ThemeEngine.Current.ResPool.GetSolidBrush( foreColor ), new Point( 24 + xPos, ( bmp.Height - e.Font.Height ) / 2 ) );
			gr.DrawImage( imageList.Images[ dcbi.ImageIndex ], new Rectangle( new Point( xPos + 2, 0 ), new Size( 16, 16 ) ) );
			
			e.Graphics.DrawImage( bmp, e.Bounds.X, e.Bounds.Y );
			gr.Dispose ();
			bmp.Dispose ();
		}
		
		protected override void OnSelectedIndexChanged( EventArgs e )
		{
			if ( Items.Count > 0 )
			{
				DirComboBoxItem dcbi = Items[ SelectedIndex ] as DirComboBoxItem;
				
				currentPath = dcbi.Path;
				
				// call DirectoryChange event only if the user changes the index with the ComboBox
				if ( !currentpath_internal_change )
				{
					if ( on_directory_changed != null )
						on_directory_changed( this, EventArgs.Empty );
				}
			}
			
			currentpath_internal_change = false;
		}
		
		public event EventHandler DirectoryChanged
		{
			add { on_directory_changed += value; }
			remove { on_directory_changed -= value; }
		}
	}
}


