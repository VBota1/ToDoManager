﻿using System;
using Gtk;
using Gdk;
using System.Collections.Generic;
using System.Linq;

namespace ToDoManager.UserInterface
{
	internal partial class MainWindow: Gtk.Window
	{
		TaskVbox IUbox;
		TaskVbox NIUbox;
		TaskVbox INUbox;
		TaskVbox NINUbox;

		internal MainWindow () : base (Gtk.WindowType.Toplevel)
		{
			Build ();
			IUbox = new TaskVbox (ImportantUrgentVBox);
			NIUbox = new TaskVbox (NotImportantUrgentVBox);
			INUbox = new TaskVbox (ImportantNotUrgentVBox);
			NINUbox = new TaskVbox (NotImportantNotUrgentVBox);

		}

		internal void showTaskList ( TaskList ptl )
		{
			TaskConvertor convert = new TaskConvertor ();
			TaskVbox tmpbox;

			foreach (var task in ptl.readTaskList() )	{
				tmpbox = identifyContainerBox (task);
				tmpbox.insertTask (convert.taskToString (task));
			}
		}

		internal bool removeTextWidget ( Task ptask )
		{
			TaskVbox tmpbox;
			tmpbox = identifyContainerBox (ptask);
			return tmpbox.removeTask (ptask.uid);
		}

		private TaskVbox identifyContainerBox (Task ptask)
		{
			TaskVbox retval;

			if ( ptask.isUrgent )	{
				if ( ptask.isImportant )		{
					retval = IUbox;
				}else{
					retval = NIUbox;
				}
			}else if ( ptask.isImportant )	{
				retval = INUbox;
			}else{
				retval = NINUbox;
			}

			return retval;
		}

		internal void setMessageText ( String pmessage )
		{
			MesssageTextView.Buffer.Text = pmessage;
		}

		protected void OnDeleteEvent (object sender, DeleteEventArgs a)
		{
			Application.Quit ();
			a.RetVal = true;
		}
			
	} 

	class TaskVbox : VBox
	{
		private VBox box;
		private List<TaskTextView> textwidgetlist = new List<TaskTextView>();

		internal TaskVbox (VBox pbox)
		{
			box = pbox;
			insertTask ( String.Empty, false);
		}

		internal bool removeTask ( int puid )
		{
			bool retval = removeTextView ( puid );

			if (0 == textwidgetlist.Count)
				insertTask (String.Empty, false);

			return retval;
		}

		internal void insertTask ( String ptaskstring )
		{
			insertTask (ptaskstring, true);
		}

		private void insertTask ( String ptaskstring, bool patstart )
		{
			removeTextView (TaskTextView.EMPTYVEIWUID);

			if ( patstart )
				box.PackStart ( initateTextView(ptaskstring) );
			else
				box.PackEnd ( initateTextView(ptaskstring) );
		}

		private bool removeTextView ( int puid )
		{
			TextView toberemoved;

			try{
				toberemoved = textwidgetlist.Single (x => x.uid == puid );
			}
			catch
			{
				return false;
			}

			box.Remove (toberemoved);

			return true;
		}

		private TextView initateTextView (String ptext)
		{
			TaskTextView retval = new TaskTextView (ptext);

			textwidgetlist.Add (retval);

			return retval;
		}

	}

	class TaskConvertor
	{
		internal String taskToString ( Task ptask )
		{
			String tmpdate = ptask.deadLine.Day + "/" + ptask.deadLine.Month + "/" + ptask.deadLine.Year;
			String retval = "UID: " + ptask.uid +
				" Title: " + ptask.title + "\n" +
				"Descritption: " + ptask.description + "\n" +
				"Estimated MD: " + ptask.durationinDays + "\n" +
				"Dead Line: " + tmpdate;
			return retval;
		}

		internal int textViewToTaskUID ( TextView pview )
		{
			return textViewToTaskUID ( pview.Buffer.Text.ToString() );
		}

		internal int textViewToTaskUID ( String pstring )
		{
			int uid;
			const int UIDPOSITION = 1;
			String[] stringvector = pstring.Split (' ');
			uid = Convert.ToInt32 (stringvector [UIDPOSITION]);
			return uid;
		}
	}

	class TaskTextView : TextView
	{
		internal int uid;
		internal const int EMPTYVEIWUID = -1;

		internal TaskTextView ( String ptext )
		{
			this.Visible = true;
			this.Editable = false;
			if (String.Empty == ptext)
				uid = EMPTYVEIWUID;
			else {
				TaskConvertor convert = new TaskConvertor ();
				uid = convert.textViewToTaskUID (ptext);
			}
			this.setText (ptext);
		}

		internal void setText (String ptext)
		{
			base.Buffer.Text = ptext;
		}

		protected override bool OnButtonPressEvent(Gdk.EventButton e) 
		{ 
			// call base (original) handler 
			base.OnButtonPressEvent(e); 
			 
			OptionsMenu options = new OptionsMenu (this);
			options.optionsmenu.Popup ();

			// return that the event was handled 
			return true; 
		}
	}

	class OptionsMenu
	{
		internal Menu optionsmenu;
		AddTaskOption addTask;
		DeleteTaskOption deleteTask;

		internal OptionsMenu ( TextView pclickedtextview )
		{
			optionsmenu = new Menu();

			addTask = new AddTaskOption ();
			optionsmenu.Append (addTask);

			deleteTask = new DeleteTaskOption (pclickedtextview);
			optionsmenu.Append (deleteTask);

			optionsmenu.ShowAll();
		}

	}

	class AddTaskOption : MenuItem
	{
		internal AddTaskOption () : base ("Add Task"){
			base.Activated += apllyActions;
		}

		internal void apllyActions (object o, EventArgs args)
		{
			TaskDefinitionWindow inputform = new TaskDefinitionWindow ();
			//throw new NotImplementedException (); 	
		}
	}

	class TaskDefinitionWindow
	{
		Gtk.Window newTaskForm = new Gtk.Window("New Task");
		Table newTaskTable = new Table (5,2,false);
		Entry titleEntry = new Entry ();
		Calendar dateEntry = new Calendar();
		SpinButton durationEntry = new SpinButton(new Adjustment (0,0,100,1,0,0),1,0);
		CheckButton importancyEntry = new CheckButton();
		TextView descriptionEntry = new TextView();

		internal TaskDefinitionWindow ()
		{
			Application.Init();
			newTaskForm.Add (newTaskTable);

			newTaskTable.Attach (new Label ("Title: "),0,1,0,1);
			newTaskTable.Attach (titleEntry, 1, 2, 0, 1);
			newTaskTable.Attach (new Label ("Description: "),0,1,1,2);
			newTaskTable.Attach (descriptionEntry, 1, 2, 1, 2);
			newTaskTable.Attach (new Label ("Duration in MD: "),0,1,2,3);
			newTaskTable.Attach (durationEntry,1,2,2,3);
			newTaskTable.Attach (new Label ("Is important: "),0,1,3,4);
			newTaskTable.Attach (importancyEntry, 1, 2, 3, 4);
			newTaskTable.Attach (new Label ("Dead Line: "),0,1,4,5);
			newTaskTable.Attach (dateEntry, 1, 2, 4, 5);


			newTaskForm.ShowAll();

			Application.Run();   
		}

		protected void OnDeleteEvent (object sender, DeleteEventArgs a)
		{
			Application.Quit ();
			a.RetVal = true;
		}
	
	}

	class DeleteTaskOption : MenuItem
	{
		TaskConvertor convert = new TaskConvertor();
		int clickedtaskuid;

		internal DeleteTaskOption ( TextView pclickedtextview ) : base ("Delete Task"){
			base.Activated += apllyActions;
			clickedtaskuid = convert.textViewToTaskUID (pclickedtextview);
		}

		internal void apllyActions (object o, EventArgs args)
		{
			MainClass.deleteTask (clickedtaskuid);
		}
	}
}
