using System;
using Mono.Data.Sqlite;
using System.Data;
using System.Collections.Generic;

//TODO: implement create table if DB does not exist

namespace ToDoManager.Database
{
	internal class DataBaseInterface
	{
		private static String dbname = "URI=file:DataRecords.sqlite";
		private static String tablename = "records";
		private static String uidcolumnname = "uid";
		private static String titlecolumnname = "title";
		private static String deadlinecolumnname = "deadLine";
		private static String descriptioncolumnname = "description";
		private static String imporntancycolumnname = "isImportant";
		private static String durationcolumnname = "durationinDays";
		private static String tablecollumns = "("+
			uidcolumnname+","+
			titlecolumnname+","+
			deadlinecolumnname+","+
			descriptioncolumnname+","+
			imporntancycolumnname+","+
			durationcolumnname+")";

		internal DataBaseInterface ()
		{
		}

		internal bool writeTaskToDB (Task ptask)
		{
			DBConnection dbcon;

			try 
			{ 
				dbcon = new DBConnection (dbname);
			}
			catch
			{
				throw;
			}

			dbcon.dbcmd.CommandText = convertTaskToSQLInsertCommand ( ptask );
			if (0 == dbcon.dbcmd.ExecuteNonQuery ())
				throw new UnableToExecuteSQLCommandException (dbcon.dbcmd.CommandText);

			return true;
		}

		private String convertTaskToSQLInsertCommand ( Task ptask )
		{
			var sqlFormattedDate = ptask.deadLine.Date.ToString("yyyy-MM-dd HH:mm:ss");

			String retval = "insert into " +
				tablename + " " +
				tablecollumns + " " +
				"values (" +
				ptask.uid + "," +
				"'" + ptask.title + "'," +
				"'" + sqlFormattedDate + "'," +
				"'" + ptask.description + "'," +
				(ptask.isImportant ? 1:0) +"," +
				ptask.durationinDays + ")";
			
			return retval;
		}

		private IDataReader readdb ( String pcmd )
		{
			DBConnection dbcon;

			try 
			{ 
				dbcon = new DBConnection (dbname);
			}
			catch
			{
				throw;
			}

			dbcon.dbcmd.CommandText = pcmd;

			try
			{
				return dbcon.dbcmd.ExecuteReader ();
			}
			catch
			{
				throw;
			}
		}

		internal List<Task> readdbToList ( )
		{
			List<Task> retval = new List<Task> ();

			String tmpcmd = "select * from " + tablename;

			IDataReader dbdata;

			try
			{
				dbdata = readdb (tmpcmd);
			}
			catch
			{
				throw;
			}

			while ( dbdata.Read () ) {
				retval.Add (convertIDataReadertoTask(dbdata));
			}

			return retval;
		}

		private Task convertIDataReadertoTask ( IDataReader pdata )
		{
			int tmpuid = Convert.ToInt32 (pdata [uidcolumnname]);
			String tmptitle = pdata[titlecolumnname].ToString ();
			DateTime tmpdeadline = Convert.ToDateTime (pdata [deadlinecolumnname]);
			int tmpduration = Convert.ToInt32 (pdata [durationcolumnname]);
			bool tmpimportancy = Convert.ToBoolean (pdata [imporntancycolumnname]);
			String tmpdesc = pdata [descriptioncolumnname].ToString ();

			Task retval = new Task (tmpuid, tmptitle,tmpdeadline,tmpduration,tmpimportancy,tmpdesc);

			return retval;
		}

		internal bool removeFromBD ( Task ptask )
		{
			DBConnection dbcon;

			try 
			{ 
				dbcon = new DBConnection (dbname);
			}
			catch
			{
				throw;
			}

			dbcon.dbcmd.CommandText = "delete from " + tablename + " where " + uidcolumnname + "=" + ptask.uid;
			if (0 == dbcon.dbcmd.ExecuteNonQuery ())
				throw new UnableToExecuteSQLCommandException (dbcon.dbcmd.CommandText);

			return true;
		}
	}

	internal class DBConnection
	{
		private static IDbConnection dbhandle;
		public IDbCommand dbcmd;

		internal DBConnection (String dbname)
		{
			dbhandle = new SqliteConnection( dbname );
			try {
				opendb ();
			} 
			catch	
			{
				throw;
			}
		}

		~DBConnection ()
		{
			closedb ();
		}

		private bool opendb ()
		{
			try {
				dbhandle.Open ();
			}
			catch ( Exception ex ) 
			{
				throw new CouldNotOpenDBException(dbhandle.ConnectionString,ex);
			}

			dbcmd = dbhandle.CreateCommand();

			return true;
		}

		private bool closedb ()
		{
			dbcmd.Dispose();
			dbhandle.Close();

			return true;
		}

	}

	internal class CouldNotOpenDBException : Exception
	{
		public CouldNotOpenDBException( String pdbname, System.Exception innerException)
			: base($"DB \"{pdbname}\" could not be oppened!",innerException)
		{}
	}

	internal class UnableToExecuteSQLCommandException : Exception
	{
		public UnableToExecuteSQLCommandException( String pcmd )
			: base($"Could not execute SQL command {pcmd}!")
		{}
	}
}

