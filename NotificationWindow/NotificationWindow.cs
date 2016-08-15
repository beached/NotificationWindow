using System.ComponentModel;
using System.Security.Permissions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Timers;
using System.Windows.Forms;

namespace NotificationWindow {
	public partial class NotificationWindow: Form {
		private static BindingList<NotificationMessage> _messages;
		private static NotificationWindow _window;
		private static System.Timers.Timer _timer;

		internal class NotificationMessage {
			internal enum NotificationType {
				Message = 0,
				Error
			}
			public string Message { get; private set; }
			public NotificationType MessageType { get; private set; }
			private readonly DateTime _timestamp;
			public DateTime Timestamp { get { return _timestamp; } }

			public NotificationMessage( string message, NotificationType messageType = NotificationType.Message ) {
				Message = message;
				_timestamp = DateTime.Now;
				MessageType = messageType;
			}
		}

		private NotificationWindow( ) {
			try {
				InitializeComponent( );
				FormBorderStyle = FormBorderStyle.None;
				SetupDataGridView( );

				if( null == _messages ) {
					_messages = new BindingList<NotificationMessage>( );
				}
				dgvMessages.DataSource = _messages;
				dgvMessages.ClearSelection( );
				SetupTimer( );
			} catch( Exception ex ) {
				Debug.WriteLine( @"Exception while constructing NotificationWindow. {0}", ex.Message );
				throw;
			}
		}

		public static void AddErrorMessage( string format, params object[] values ) {
			AddMessage( NotificationMessage.NotificationType.Error, format, values );
		}

		public static void AddMessage( string format, params object[] values ) {
			AddMessage( NotificationMessage.NotificationType.Message, format, values );
		}

		private static void AddMessage( NotificationMessage.NotificationType messageType, string messageFormat, params object[] messageValues ) {			
			try {
				CreateWindowIfNeeded( );				
				AddMessageToQueue( messageType, messageFormat, messageValues );
				SetWindowColour( );
			} catch( Exception ex ) {
				Debug.WriteLine( string.Format( @"Error adding message. {0}", ex.Message ) );				
			}
		}

		public new int Right {
			get { return base.Right; }
			set {
				Helpers.Assert( 0 <= value, @"Right must be at least 0" );
				Left = value - Width;
			}
		}

		public new int Bottom {
			get { return base.Bottom; }
			set {
				Helpers.Assert( 0 <= value, @"Bottom must be at least 0" );
				Top = value - Height;
			}
		}

		protected override bool ShowWithoutActivation {
			get { return true; }
		}

		[Flags]
		private enum Win32 {
			WsExNoactivate = 0x08000000,
			WsExToolwindow = 0x00000080,
			WsThickframe = 0x00040000,
			WsExTopmost = 0x00000008,
		}

		protected override CreateParams CreateParams {
			[SecurityPermission( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
			get {
				var baseParams = base.CreateParams;				
				baseParams.ExStyle |= (int)(Win32.WsExNoactivate | Win32.WsExToolwindow | Win32.WsExTopmost );
				baseParams.Style |= (int)(Win32.WsThickframe);
				return baseParams;
			}
		}

		private void SetRight( int value ) {
			InvokeIfNeeded( ( ) => { Right = value; } );
		}

		private void SetBottom( int value ) {
			InvokeIfNeeded( ( ) => { Top = value - Height; } );
		}

		private void SetupDataGridView( ) {
			dgvMessages.AllowUserToAddRows = false;
			dgvMessages.AllowUserToDeleteRows = false;
			dgvMessages.AllowUserToOrderColumns = false;
			dgvMessages.AllowUserToResizeColumns = false;
			dgvMessages.AllowUserToResizeRows = false;
			dgvMessages.ReadOnly = true;
			dgvMessages.Enabled = false;
			dgvMessages.RowHeadersVisible = false;
			dgvMessages.ColumnHeadersVisible = false;
			dgvMessages.AutoGenerateColumns = false;
			dgvMessages.TabStop = false;
			{
				var column = Helpers.MakeColumn( @"Message" );
				column.DefaultCellStyle.Font = Properties.Settings.Default.MessageFont;
				column.DefaultCellStyle.WrapMode = DataGridViewTriState.True;				
				dgvMessages.Columns.Add( column );
			}
			dgvMessages.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;			

		}

		private static void ShutdownMessages( ) {
			StopTimer( );
			WithMessage( ( ) => {
				try {
					if( null == _messages ) {
						return;
					}
					_messages.Clear( );
					_messages = null;
				} catch( Exception ex ) {
					Debug.WriteLine( string.Format( @"Exception while disposing of _messages. {0}", ex.Message ) );
				}
			} );			
		}

		private void CloseForm( int millisecondsToClose ) {
			ShutdownMessages( );
			FadeAway( millisecondsToClose );
			Close( );
		}

		private void FadeAway( int millisecondsToWork ) {
			try {
				if( 0 == millisecondsToWork ) {
					SetOpacity( 0.0 );
					return;
				}
				var timeBetweenFrames = (int)(millisecondsToWork / 100.0);
				while( Opacity > 0 ) {
					SetOpacity( Opacity - 0.01 );
					Thread.Sleep( timeBetweenFrames );
					timeBetweenFrames = timeBetweenFrames % 2 == 0 ? timeBetweenFrames - 1 : timeBetweenFrames;
					timeBetweenFrames = timeBetweenFrames >= 0 ? timeBetweenFrames : 0;
				}
			} catch( Exception ex ) {
				Debug.WriteLine( string.Format( @"Error while setting fading. {0}", ex.Message ) );
			} finally {
				try {
					SetOpacity( 0.0 );
				} catch( Exception ex ) {
					Debug.WriteLine( string.Format( @"Error while setting opacity. {0}", ex.Message ) );
				}
			}
		}

		private void NotificationWindow_Load( object sender, EventArgs e ) {
			SetRight( Screen.GetWorkingArea( this ).Right );
			SetBottom( Screen.GetWorkingArea( this ).Bottom );
		}

		private void NotificationWindow_Shown( object sender, EventArgs e ) {			
			StartTimer( );
			_window.Click += ( obj, arg ) => InvokeIfNeeded( ( ) => CloseForm( 250 ) );
		}

		private void InvokeIfNeeded( Action action ) {
			if( InvokeRequired ) {
				Invoke( action );
			} else {
				action( );
			}
		}

		private void SetOpacity( double percentOpac ) {
			InvokeIfNeeded( ( ) => { Opacity = percentOpac; } );
		}

		// Static Methods	
		private static void AddMessageToQueue( NotificationMessage.NotificationType messageType, string messageFormat, params object[] messageValues ) {
			var message = string.Format( messageFormat, messageValues );
			WithMessage( ( ) => _messages.Add( new NotificationMessage( message, messageType ) ) );
		}

		private static void CreateWindowIfNeeded( ) {
			if( null != _window ) {
				return;
			}
			_window = new NotificationWindow( );
			_window.Show( );
		}

		private static void OnWindow( Action<NotificationWindow> action ) {
			if( null != _window ) {
				_window.InvokeIfNeeded( ( ) => action( _window ) );
			}
		}

		private static void ClearSelection( ) {
			OnWindow( delegate( NotificationWindow window ) {
				window.dgvMessages.ClearSelection( );
				window.dgvMessages.Update( );
			} );
		}

		private static void ShouldIStayOpen( object sender, ElapsedEventArgs args ) {
			OnWindow( window => {
				if( 0 < CleanupMessages( Properties.Settings.Default.MessageTimeoutMilliseconds ) ) {
					StartTimer( );
					return;
				}
				window.CloseForm( Properties.Settings.Default.MessagePollEveryMilliseconds );
				_window = null;
			} );
		}

		private static void WithMessage( Action action ) {
			var isTimerEnabled = StopTimer( );
			try {
				OnWindow( window => {
					action( );
					window.dgvMessages.ClearSelection( );
					window.dgvMessages.Update( );
				} );
			} finally {
				if( isTimerEnabled ) {
					StartTimer( );
				}
			}
		}

		private static void SetupTimer( ) {
			_timer = new System.Timers.Timer( Properties.Settings.Default.MessagePollEveryMilliseconds );
			_timer.Elapsed += ShouldIStayOpen;
		}

		private static void StartTimer( ) {
			if( null == _timer ) {
				return;
			}
			_timer.Enabled = true;
		}

		private static bool StopTimer( ) {
			if( null == _timer ) {
				return false;
			}
			var result = _timer.Enabled;
			_timer.Enabled = false;
			return result;
		}

		private static bool HasErrorMessage( IEnumerable<NotificationMessage> messages ) {
			return null != messages && messages.Any( message => NotificationMessage.NotificationType.Error == message.MessageType );
		}

		private static int CleanupMessages( int maxAgeMilliseconds ) {
			var now = DateTime.Now;
			WithMessage( ( ) => _messages.RemoveAll( message => (now - message.Timestamp).TotalMilliseconds >= maxAgeMilliseconds ) );
			OnWindow( window => SetWindowColour( ) );
			return _messages.Count;
		}

		private static void SetBackgroundColour( Color colour ) {
			OnWindow( delegate( NotificationWindow window ) {
				window.BackColor = colour;
				window.dgvMessages.BackColor = colour;
				window.dgvMessages.DefaultCellStyle.BackColor = colour;
				window.dgvMessages.BackgroundColor = colour;
				window.dgvMessages.RowsDefaultCellStyle.BackColor = colour;				
				window.dgvMessages.DataSource = null;
				window.dgvMessages.DataSource = _messages;				
			} );
			ClearSelection(  );
		}

		private static void SetWindowColour( ) {
			SetBackgroundColour( HasErrorMessage( _messages ) ? Properties.Settings.Default.BgColourError : Properties.Settings.Default.BgColourNormal );
			OnWindow( window => window.Refresh( ) );
			ClearSelection( );
		}
	}



}
