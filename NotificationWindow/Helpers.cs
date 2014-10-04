using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Contracts;

namespace NotificationWindow {
	public class Helpers {
		public class AssertionException: Exception {
			public AssertionException( ): base( ) { }
			public AssertionException( string message ) : base( message ) { }
		}

		public static void Assert( bool condition, string messageFormat, params object[] messageValues ) {
			if( !condition ) {
				throw new AssertionException( string.Format( messageFormat, messageValues ) );
			}
		}

		
	}
	
	public static class Extension {
		public static void RemoveAll<T>( this BindingList<T> values, Predicate<T> predicate ) {
			var itemsToRemove = new List<int>( );
			for( var n = values.Count - 1; n >= 0; --n ) {
				if( predicate( values[n] ) ) {
					values.RemoveAt( n );
				}
			}

		}
	}
}
