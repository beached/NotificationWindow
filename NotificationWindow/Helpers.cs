using System;
using System.Windows.Forms;

namespace NotificationWindow {
	public static class Helpers {
		public static void Assert( bool condition, string messageFormat, params object[] messageValues ) {
			if( !condition ) {
				throw new AssertionException( string.Format( messageFormat, messageValues ) );
			}
		}

		public static DataGridViewColumn MakeColumn( string propertyName, string headerName = null, bool hidden = false, bool canSort = true, bool readOnly = true ) {
			return new DataGridViewColumn {
				Name = propertyName, AutoSizeMode =DataGridViewAutoSizeColumnMode.Fill, HeaderText = (headerName ?? propertyName), ReadOnly = readOnly, DataPropertyName = propertyName, SortMode = (canSort ? DataGridViewColumnSortMode.Automatic : DataGridViewColumnSortMode.NotSortable), CellTemplate = new DataGridViewTextBoxCell( ), Visible = !hidden
			};
		}

		public static void AddColumn( DataGridView dgv, string propertName, string headerName = null, bool hidden = false, bool canSort = true, bool readOnly = true ) {
			dgv.Columns.Add( MakeColumn( propertName, headerName, hidden, canSort, readOnly ) );
		}		
	}

	[Serializable]
	public class AssertionException: Exception {
		public AssertionException( ) { }
		public AssertionException( string message ) : base( message ) { }
	}

}
