Notification Window
==================

[![Join the chat at https://gitter.im/beached/NotificationWindow](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/beached/NotificationWindow?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

A .Net Library to display a pop-up notification in the style of IM clients without forcing the user to respond or stealing focus.  It will automatically disappear after a predetermined amount of time. 

It is called by one of two static methods void AddMessage( string format, params object[] values ) and void AddErrorMessage( string format, params object[] values )

The parameters are like string.format.

e.g.

NotificationWindow.AddMessage( @"Your task completed at {0}", DateTime.Now ); 
