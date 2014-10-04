using System;
using System.Drawing;
using System.Threading;
using System.Timers;
using System.Windows.Forms;
using SyncList;

namespace NotificationWindow {
	public partial class NotificationWindow: Form {
		private static SyncList<NotificationMessage> _messages;
		private static NotificationWindow _window;
		private System.Timers.Timer _timer;
		private static string _lock = @"IAMLOCKED";

		internal class NotificationMessage {
			public string Message { get; private set; }
			
			private readonly DateTime _timestamp;
			public DateTime Timestamp { get { return _timestamp; } }

			public NotificationMessage( string message ) {
				Message = message;
				_timestamp = DateTime.Now;
			}
		}

		public static void AddMessage( string format, params object[] values ) {
			var message = string.Format( format, values );
			if( null == _window ) {
				_window = new NotificationWindow( );
				_window.Show( );
			}
			lock( _lock ) {
				_messages.Add( new NotificationMessage( message ) );
			}
		}

		private NotificationWindow( ) {
			InitializeComponent( );
			FormBorderStyle = FormBorderStyle.None;
			SetupDataGridView( );

			_messages = new SyncList<NotificationMessage>( this );
			dgvMessages.DataSource = _messages;
		}

		private void SetupDataGridView( ) {
			dgvMessages.AllowUserToAddRows = false;
			dgvMessages.AllowUserToDeleteRows = false;
			dgvMessages.AllowUserToOrderColumns = false;
			dgvMessages.AllowUserToResizeColumns = false;
			dgvMessages.AllowUserToResizeRows = false;
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

		private void ShouldIStayOpen( Object source, ElapsedEventArgs e ) {
			var messagesLeft = CleanMessages( 4 );
			if( 0 != messagesLeft || null == _window ) {
				_timer.Enabled = true;
				return;
			}
			_window.CloseForm( 1000 );
			_window = null;
		}

		private void CloseForm( int millisecondsToClose = 0 ) {
			new Thread( ( ) => {
				lock( _lock ) {
					if( null != _messages && 0 < _messages.Count ) {
						_messages.Clear( );
						_messages = null;
					}
				}
				FadeAway( millisecondsToClose );
				//Shrink( 5, 5, true );
				InvokeIfNeeded( ( ) => {
					Close( );
					Dispose( );
				} );
			} ).Start( );
		}

		private void FadeAway( int millisecondsToWork ) {
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
		}

		private void NotificationWindow_Load( object sender, EventArgs e ) {
			SetRight( Screen.GetBounds( this ).Right );
			SetBottom( Screen.GetWorkingArea( this ).Bottom );
		}

		private void NotificationWindow_Shown( object sender, EventArgs e ) {
			_timer = new System.Timers.Timer( 1000 );
			_timer.Elapsed += ShouldIStayOpen;
			_timer.Enabled = true;
		}
		
		private static int CleanMessages( int maxAgeSeconds ) {
			if( null == _messages ) {
				return 0;
			}
			lock( _lock ) {
				var now = DateTime.Now;
				_messages.RemoveAll( message => (now - message.Timestamp).TotalSeconds >= maxAgeSeconds );
				return _messages.Count;
			}
		}

		private void InvokeIfNeeded( Action action ) {
			if( InvokeRequired ) {
				Invoke( action );
			} else {
				action( );
			}
		}

		private void SetHeight( int position ) {
			Helpers.Assert( 0 <= position, @"Heights must be at least 0" );
			Helpers.Assert( Screen.GetWorkingArea( this ).Height >= position, "Height is out of bounds" );
			InvokeIfNeeded( ( ) => { Height = position; } );
		}

		private void SetRight( int position ) {
			Helpers.Assert( 0 <= position, @"Right must be at least 0" );
			Helpers.Assert( Screen.GetWorkingArea( this ).Right >= position, "Right is out of bounds" );
			
			InvokeIfNeeded( ( ) => {
				Left = position - Width;
			} );
		}

		private void SetBottom( int position ) {
			Helpers.Assert( 0 <= position, @"Bottom must be at least 0" );
			Helpers.Assert( Screen.GetWorkingArea( this ).Bottom >= position, "Bottom is out of bounds" );

			InvokeIfNeeded( ( ) => {
				Top = position - Height;
			} );
		}

		private void SetDockState( Control ctrl, bool isDocked ) {
			if( null != ctrl && !ctrl.IsDisposed ) {
				InvokeIfNeeded( ( ) => { ctrl.Dock = isDocked ? DockStyle.Fill : DockStyle.None; } );
			}
		}

		private void SetVisible( Control ctrl, bool isVisible ) {
			if( null != ctrl && !ctrl.IsDisposed ) {
				InvokeIfNeeded( ( ) => { ctrl.Visible = isVisible; } );
			}
		}

		private void SetOpacity( double percentOpac ) {
			InvokeIfNeeded( ( ) => { Opacity = percentOpac; } );
		}
	}



}
