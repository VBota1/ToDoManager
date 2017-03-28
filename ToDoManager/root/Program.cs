using System;
using Gtk;
using ToDoManager.UserInterface;
using ToDoManager.Database;
using System.Collections.Generic;
using System.Linq;

namespace ToDoManager
{
	class MainClass
	{
		static MainWindow win;

		internal static void Main (string[] args)
		{
			
			Application.Init ();
			win = new MainWindow ();

			try
			{
				TaskList tasks = new TaskList();
				/*
				Random rndm = new Random();
				DateTime duedate = new DateTime(DateTime.Today.Year,(DateTime.Today.Month+1),rndm.Next(1,28));
				tasks.addTask ("test1",duedate,rndm.Next (0,30),generateNextBool (),"automaticaly added text");
				DateTime duedate2 = new DateTime(DateTime.Today.Year,(DateTime.Today.Month+1),rndm.Next(1,28));
				tasks.addTask ("test me now",duedate2,rndm.Next (0,30),generateNextBool (),"automaticaly added text");
				tasks.removeTask (tasks.readTaskList ().Single (x => x.uid == 2));
*/
				win.showTaskList (tasks);
			}
			catch ( Exception ex )
			{
				win.setMessageText (ex.Message);
			}

			win.Show ();
			Application.Run ();
		}

		internal static void deleteTask (int puid)
		{
			Task tobedeleted;

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
				win.setMessageText (ex.Message);
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

		internal bool addTask ( String pTitle, DateTime? pdeadline = null, int pduration = 0, bool pImportant = false, String pDescription = "no description" )
		{
			int uid;
			try{
				
				uid = getUniqueUID();;
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

			return true;
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
			: base($"Task List is full! Please delete some taks before creating new ones.")
		{}
	}

	internal class TaskNotFoundException : Exception
	{
		public TaskNotFoundException(int puid)
			: base($"Task with UID: {puid} was not found!")
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
