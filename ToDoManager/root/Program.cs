using System;
using Gtk;
using ToDoManager.UserInterface;
using ToDoManager.Database;
using System.Collections.Generic;
using System.Linq;

//TODO: fix problem with rethrow message

namespace ToDoManager
{
	class MainClass
	{
		static internal MainWindow win;
		static TaskList tasks;

		internal static void Main (string[] args)
		{
			
			Application.Init ();
			win = new MainWindow ();

			try
			{
				tasks = new TaskList();
				win.showTaskList (tasks);
			}
			catch ( Exception ex )
			{
				printExceptionInfo (ex);
			}

			win.Show ();
			Application.Run ();
		}

		internal static void printExceptionInfo (Exception pex)
		{
			String innexmessage = String.Empty;
			if ( null != pex.InnerException )
				innexmessage = " --> " + pex.InnerException.Message;
				
			String exmessage = pex.Message + innexmessage;
			win.setMessageText (exmessage);
		}

		internal static void addTask ( String pTitle, DateTime? pdeadline = null, int pduration = 0, bool pImportant = false, String pDescription = "no description" )
		{
			int newuid=TaskTextView.EMPTYVEIWUID;

			if (null == tasks) {
				win.setMessageText ("Task list is not initialized! Check Data Base.");
				return;
			}

			try
			{
				newuid = tasks.addTask (pTitle,pdeadline,pduration,pImportant,pDescription);
			}
			catch (Exception ex)
			{
				printExceptionInfo (ex);
				return;
			}

			win.showTask (tasks.readTaskList ().Single (x => x.uid ==newuid));
		}

		internal static void deleteTask (int puid)
		{
			Task tobedeleted;

			if ( TaskTextView.EMPTYVEIWUID == puid )
			{
				win.setMessageText("No task to be deleted! Try adding some tasks first.");
				return;
			}

			try 
			{
				TaskList tmplist = new TaskList ();
				tobedeleted = tmplist.readTaskList().Single (x => x.uid == puid);
			}
			catch
			{
				throw new TaskNotFoundException(puid);
			}

			try 
			{
				TaskList.removeTask (tobedeleted);
			}
			catch ( Exception ex )
			{
				printExceptionInfo (ex);
			}

			win.removeTextWidget (tobedeleted);
		}

		private static bool generateNextBool ()
		{
			Random rndm = new Random ();
			if (rndm.NextDouble() >= 0.5)
				return true;
			else
				return false;
		}
	}

	class TaskList
	{
		static private List<Task> taskList = new List<Task>();
		static private DataBaseInterface dbhandle;

		internal int addTask ( String pTitle, DateTime? pdeadline = null, int pduration = 0, bool pImportant = false, String pDescription = "no description" )
		{
			int uid;
			try{
				
				uid = getUniqueUID();
			}
			catch (TaskListIsFullException)
			{
				throw;
			}

			Task tmptask = new Task (uid, pTitle, pdeadline, pduration, pImportant, pDescription);

			try
			{
				dbhandle.writeTaskToDB (tmptask);
			}
			catch
			{
				throw;
			}

			taskList.Add (tmptask);

			return uid;
		}

		static internal bool removeTask ( Task ptask )
		{
			try
			{
				dbhandle.removeFromBD ( ptask );
			}
			catch
			{
				throw;
			}

			taskList.Remove ( ptask );

			return true;
		}

		internal IReadOnlyList<Task> readTaskList ()
		{
			return taskList.AsReadOnly ();
		}

		private int getUniqueUID ()
		{
			
			for (int i = 0; i < int.MaxValue; i++)
				if (!taskList.Any (x => x.uid == i))
					return i;
			
			throw new TaskListIsFullException();
		}

		static TaskList ()
		{
			dbhandle = new DataBaseInterface ();
			try
			{
				taskList = dbhandle.readdbToList ();
			}
			catch
			{
				throw;
			}
		}
	}

	internal class TaskListIsFullException : Exception
	{
		public TaskListIsFullException() 
			: base("Task List is full! Please delete some taks before creating new ones.")
		{}
	}

	internal class TaskNotFoundException : Exception
	{
		public TaskNotFoundException(int puid)
			: base($"Task with UID: {puid} was not found!")
		{}
	}

	internal class CouldNotCreateTaskListException : Exception
	{
		public CouldNotCreateTaskListException(String arg)
			: base ($"Task List Could not be created: " + arg)
		{}
	}


	internal class Task
	{
		internal int uid { get; }
		internal String title { get; set; }
		internal String description { get; set; }
		internal bool isUrgent { get; set; }
		internal bool isImportant { get; set; }
		private DateTime _deadline;
		internal DateTime deadLine 
		{ 
			get{ return _deadline; } 
			set{
				if (value >= DateTime.Today)
					_deadline = value;
				else
					_deadline = DateTime.Today;
			} 
		}
		private int _durationindays;
		internal int durationinDays 
		{ 
			get { return _durationindays; }
			set{
				if ( value >=0 )
					_durationindays = value;
				else
					_durationindays = 0;
			}
		}

		internal Task ( int puid, String pTitle, DateTime? pdeadline = null, int pduration = 0, bool pImportant = false, String pDescription = "no description" )
		{
			DateTime ldeadline = pdeadline ?? DateTime.Today;

			uid = puid;
			title = pTitle;
			deadLine = ldeadline;
			durationinDays = pduration;
			isImportant = pImportant;
			description = pDescription;
			isUrgent = calculateUrgency ();
		}

		private bool calculateUrgency ()
		{
			if ( deadLine <= DateTime.Today.AddDays (durationinDays) ) 
				return true;
			else 
				return false;
		}

		internal void updateUrgency ()
		{
			isUrgent = calculateUrgency ();
		}
	}
}
