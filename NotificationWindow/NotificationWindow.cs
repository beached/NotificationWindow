using System.Timers;
using SyncList;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace NotificationWindow {
	public partial class NotificationWindow: Form {
		private static SyncList<NotificationMessage> _messages;
		private static NotificationWindow _window;
		private static System.Timers.Timer _timer;
		private static readonly Object MessagesLock = new object( );

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
					_messages = new SyncList<NotificationMessage>( this );
				}
				dgvMessages.DataSource = _messages;
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
				OnWindow( window => window.CloseForm( 0 ) );
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
			{
				var column = Helpers.MakeColumn( @"Message" );
				column.DefaultCellStyle.Font = new Font( FontFamily.GenericSansSerif, 16 );
				dgvMessages.Columns.Add( column );
			}

			dgvMessages.Click += delegate {		// If use click on window, close
				InvokeIfNeeded( ( ) => CloseForm( 250 ) );
			};
		}

		private void CloseForm( int millisecondsToClose = 0 ) {
			StopTimer( );
			WithMessageLock( ( ) => {
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
			new Thread( ( ) => {
				WithMessageLock( ( ) => _messages.Add( new NotificationMessage( message, messageType ) ) );
				OnWindow( delegate {
					SetWindowColour( );
					ClearSelection( );
				} );
			} ).Start( );
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
				if( 0 < CleanMessages( 3 ) ) {
					StartTimer( );
					return;
				}
				window.CloseForm( 1000 );
				_window = null;
			} );
		}

		private static void WithMessageLock( Action action ) {
			var isTimerEnabled = StopTimer( );
			try {
				lock( MessagesLock ) {
					action( );
				}
			} finally {
				if( isTimerEnabled ) {
					StartTimer( );
				}
			}
		}

		private static void SetupTimer( ) {
			_timer = new System.Timers.Timer( 1000 );
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

		private static int CleanMessages( int maxAgeSeconds ) {
			var now = DateTime.Now;
			WithMessageLock( ( ) => _messages.RemoveAll( message => (now - message.Timestamp).TotalSeconds >= maxAgeSeconds ) );
			OnWindow( window => SetWindowColour( ) );
			return _messages.Count;
		}

		private static void SetBackgroundColour( Color colour ) {
			OnWindow( delegate( NotificationWindow window ) {
				window.BackColor = colour;
				window.dgvMessages.BackColor = colour;
				window.dgvMessages.DefaultCellStyle.BackColor = colour;
				window.dgvMessages.RowsDefaultCellStyle.BackColor = colour;
				window.dgvMessages.BackgroundColor = colour;
				window.dgvMessages.DataSource = null;
				window.dgvMessages.DataSource = _messages;
			} );
		}

		private static void SetWindowColour( ) {
			SetBackgroundColour( HasErrorMessage( _messages ) ? Color.OrangeRed : Color.Khaki );
			OnWindow( window => window.Refresh( ) );
			ClearSelection( );
		}
	}



}
