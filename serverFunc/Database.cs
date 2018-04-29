using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Configuration;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Linq;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using serverFunc;

namespace database
{
    public partial class mySQLClass
    {
        //max length(number of bytes for a string)) of a column in one table 
        const int LEN_1 = 1;
        const int LEN_4 = 4;
        const int LEN_8 = 8;
        const int LEN_40 = 40;
        const int LEN_100 = 100;
        const int LEN_200 = 200;

        const int EXCEL_FIRSTLINE_DATA = 0;  //read every line to data table
        const int EXCEL_FIRSTLINE_TITLE = 1; //so we will not read first line into table

        const int DECREASE_ORDER = 0;  //output in decreasing order 
        const int INCREASE_ORDER = 1;  //output in increasing order 

        public const string sampleDatabaseName = "sampleDatabase";

        public const string employeeTableName = "employee";
        public const string employeeFileName = "..\\..\\data\\basicData\\employee.xlsx";
        public const string materialTableName = "material";
        public const string materialFileName = "..\\..\\data\\basicData\\material.xlsx";
        public const string productTableName = "productList";
        public const string productFileName = "..\\..\\data\\basicData\\productList.xlsx";
        public const string machineTableName = "machineList";
        public const string machineFileName = "..\\..\\data\\basicData\\machineList.xlsx";
        public const string UI_InputTableName = "UI_Input";
        public const string UI_InputFileName = "..\\..\\data\\basicData\\UI_Input.xlsx";
        public const string bomTableName = "bomList";
        public const string bomFileName = "..\\..\\data\\basicData\\bom.xlsx";

        public const int NOT_APPEND_RECORD = 0;  //this is not an append action
        public const int APPEND_RECORD = 1;  //this is an append action, so we need to consider the index in database for the last appended record

        static object writeNewRecordLocker = new object();
        static object modifyRecordLocker = new object();

        public static string[] infoTableName = 
        { 
          	employeeTableName, materialTableName, productTableName, machineTableName, bomTableName, 
        };

        public static string[] infoTableFileName = 
        {
            employeeFileName, materialFileName, productFileName, machineFileName, UI_InputFileName, bomFileName, 
        };

        public static string connectionString;

        public mySQLClass()
        {
//            string str;
            MySqlConnection myConnection;

            connectionString = "data source = localhost; user id = root; PWD = ; Charset=utf8";

            try
            {
                myConnection = new MySqlConnection(connectionString);
                myConnection.Open();
                myConnection.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("connection fail!" + ex);
                MessageBox.Show("抱歉，请确认 wamp server 已启动，否则本系统无法运行。", "信息提示", MessageBoxButtons.OK);
                System.Environment.Exit(0);
            }
        }


        //clear all database and generate basic info table database for this project
        public static void buildBasicDatabase()
        {
            int i;

            try
            {
                //basic info
                //deleteDatabase(sampleDatabaseName);
                if (is_DatabseExist(sampleDatabaseName))	return;

                createDatabase(sampleDatabaseName);

                for (i = 0; i < infoTableName.Length; i++)  //need to look for sample data in excel files
                {
                        createDataTableFromExcel(sampleDatabaseName, infoTableName[i], infoTableFileName[i], LEN_40, 1);
                }

                for (i = 1; i <= gVariable.MAX_CLIENT_ID; i++)
                {
                    createDataTableFromExcel(sampleDatabaseName, UI_InputTableName + i, UI_InputFileName, LEN_40, 0);  //for UI_Input table, only genrate table structure, no data
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("buildBasicDatabase failed!" + ex);
            }
        }

		public static bool is_DatabseExist(String databaseName)
        {
            MySqlConnection myConnection = new MySqlConnection(connectionString);

        	try {
				MySqlCommand cmd = new MySqlCommand("use " + databaseName, myConnection);
				myConnection.Open();
                cmd.ExecuteNonQuery();
				myConnection.Close();
				return true;
        	}
			catch (Exception ex) {
				myConnection.Close();
				return false;
			}
		}

        public static void createDatabase(String databaseName)
        {
            try
            {
                String createString = "create database IF NOT EXISTS " + databaseName;

                MySqlConnection myConnection = new MySqlConnection(connectionString);

                MySqlCommand cmd = new MySqlCommand(createString, myConnection);
                myConnection.Open();
                cmd.ExecuteNonQuery();
                myConnection.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("database " + databaseName + "generating fail!" + ex);
            }
        }


        //create a new table under a certain database
        public static void createDataTable(String databaseName, string createDataTableString)
        {
            try
            {
                String createString;

                //when table name has special characters like '-', we may fail in table processing, like a table name of aa-bb, we need to set it to `aa-bb' to deal with a table
                createString = "create table IF NOT EXISTS `" + createDataTableString;

                MySqlConnection myConnection = new MySqlConnection("database = " + databaseName + "; " + connectionString);
                MySqlCommand cmd = new MySqlCommand(createString, myConnection);
                myConnection.Open();
                cmd.ExecuteNonQuery();
                myConnection.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(createDataTableString + " generating fail!" + ex);
            }
        }

        public static void deleteDatabase(String databaseName)
        {
            try
            {
                String createString = "drop database if exists " + databaseName;

                MySqlConnection myConnection = new MySqlConnection(connectionString);
                MySqlCommand cmd = new MySqlCommand(createString, myConnection);
                myConnection.Open();
                cmd.ExecuteNonQuery();
                myConnection.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("database " + databaseName + "Delete database fail!" + ex);
            }
        }

        public static void clearTable(String databaseName, String tableName)
        {
            String str;
            MySqlCommand cmd;
            MySqlConnection myConnection;

            try
            {
                //delete and truncate are similar, but after delete function, new ID will still follow the original ID, while after truncate, ID will start from 1  
                //                str = "delete from `" + tableName + "`";
                str = "truncate table `" + tableName + "`";
                myConnection = new MySqlConnection("database = " + databaseName + "; " + connectionString);
                cmd = new MySqlCommand(str, myConnection);
                myConnection.Open();
                cmd.ExecuteNonQuery();
                myConnection.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("database " + databaseName + "Clear table fail!" + ex);
            }
        }

        public static void deleteTable(string databaseName, String tableName)
        {
            try
            {
                String clearDataTableString = "drop table if exists `" + tableName + "`";

                MySqlConnection myConnection = new MySqlConnection("database = " + databaseName + "; " + connectionString);
                MySqlCommand cmd = new MySqlCommand(clearDataTableString, myConnection);
                myConnection.Open();
                cmd.ExecuteNonQuery();
                myConnection.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("drop table fail!" + ex);
            }
        }


        public static int getRecordNumInTable(String databaseName, String tableName)
        {
            int num;

            num = 0;
            try
            {
                MySqlConnection myConnection = new MySqlConnection("database = " + databaseName + "; " + connectionString);
                myConnection.Open();

                MySqlCommand myCommand = myConnection.CreateCommand();

                //when table name has special characters like '-', we may fail in table processing, like a table name of aa-bb, we need to set it to `aa-bb' to deal with a table
                myCommand.CommandText = "select count(*) from `" + tableName + "`";
                num = Convert.ToInt32(myCommand.ExecuteScalar());

                myConnection.Close();

                return num;
            }
            catch (Exception ex)
            {
                Console.WriteLine("database " + databaseName + "Get record num in table " + tableName + " failed!" + ex);
                return -1;
            }
        }

        //could be update or delete a record, function controlled by modifyString
        public static int modifyTableRecord(String databaseName, String modifyString)
        {
            int ret;

            lock (modifyRecordLocker)
            {
                ret = 0;
                try
                {
                    MySqlConnection myConnection = new MySqlConnection("database = " + databaseName + "; " + connectionString);
                    myConnection.Open();

                    MySqlCommand myCommand = myConnection.CreateCommand();

                    myCommand.CommandText = modifyString;

                    myCommand.ExecuteNonQuery();
                    myConnection.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("updateTableRecordContent Failed for: " + modifyString + ". " + ex);
                    ret = -1;
                }

                return ret;
            }
        }


        //append a new record to database table
        //databaseName :
        //tablename    :
        //excelFileName:
        //dataArray    : We need to put all informations concerning this record into this array before we call thius function, the format/content should be the same as excel file
        //return: > 0, ID of the last appended record
        //       == 0, should never happen
        //         -1, failed
        public static int addNewRecordToTable(string databaseName, string tableName, string excelFileName, string [] dataArray)
        {
            int i;
            int ret;
            int columnNum;
            string insertString;
            DataTable excelTable;
            MySqlConnection myConnection;
            MySqlParameter[] param;

            ret = 0;
            myConnection = null;
            lock (writeNewRecordLocker)
            {
                try
                {
                    if (dataArray == null)
                    {
                        Console.WriteLine("addNewRecordToTable(): database " + databaseName + "." + tableName + " failed!" + "input array of dataArray is null");
                        return -1;
                    }

                    excelTable = readExcelToDataTable(excelFileName, EXCEL_FIRSTLINE_TITLE);
                    if (excelTable == null)
                    {
                        Console.WriteLine("addNewRecordToTable(): database " + databaseName + "." + tableName + " failed!" + excelFileName + " not found or format error!");
                        return -1;
                    }

                    columnNum = dataArray.Length;

                    //columnNum does not include ID column
                    param = new MySqlParameter[columnNum + 1];
                    param[0] = new MySqlParameter("@id", 0);

                    insertString = " value(@id";

                    foreach (DataRow dr in excelTable.Rows)
                    {
                        for (i = 0; i < dr.ItemArray.Length; i++)
                        {
                            insertString += ",@" + dr.ItemArray[i];
                            param[i + 1] = new MySqlParameter("@" + dr.ItemArray[i], dataArray[i]);
                        }
                        insertString += ")";
                        break;
                    }
                    
                    myConnection = new MySqlConnection("database = " + databaseName + "; " + connectionString);
                    myConnection.Open();
                    MySqlCommand myCommand = myConnection.CreateCommand();
                    myCommand.CommandText = "insert into `" + tableName + "` " + insertString;
                    myCommand.Parameters.AddRange(param);
                    myCommand.ExecuteNonQuery();

                    ret = (int)myCommand.LastInsertedId;
                    myConnection.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("addNewRecordToTable(): database " + databaseName + "." + tableName + " failed!" + ex);

                    if (myConnection != null)
                        myConnection.Close();

                    ret = -1;
                }
            }
            return ret;
        }

        //get title string(the first line of an excel file, normally are all Chinese characters in our project) from an excel file, normally we use this function to get all titles for a listview 
        //return: number of items in this table
        public static int getListTitleFromExcel(string fileName, ref string [] titleArray)
        {
            int i;
            DataTable excelTable;

            i = -1;
            try
            {
                if (fileName == null)
                    return -1;

                excelTable = readExcelToDataTable(fileName, EXCEL_FIRSTLINE_DATA);
                if (excelTable == null)
                    return -1;

                foreach (DataRow dr in excelTable.Rows)
                {
                    titleArray = new string[dr.ItemArray.Length];

                    for (i = 0; i < dr.ItemArray.Length; i++)
                    {
                        titleArray[i] = dr.ItemArray[i].ToString();
                    }
                    break;
                }

                return i;
            }
            catch (Exception ex)
            {
                Console.WriteLine("getListTitleFromExcel() failed in get title string from " + fileName + "! " + ex);
                return -1;
            }
        }

        //get create string for a database table from its excel file, this string is used to create a new database table.
        //a normal table creating string looks like:"create table tableName (id int(1) AUTO_INCREMENT primary key, item1 varchar(40), item2 varchar(40), item3 int(1)) ENGINE = MYISAM CHARSET=utf8";
        //return: number of items in this table
        public static int getDatabaseCreateStringFromExcel(string tableName, ref string createString, string fileName, int itemLength)
        {
            int i;
            string defaultTableItem = " varchar(" + itemLength + ")";
            DataTable excelTable;

            i = -1;
            try
            {
                if (fileName == null)
                    return -1;

                excelTable = readExcelToDataTable(fileName, EXCEL_FIRSTLINE_TITLE);
                if (excelTable == null)
                    return -1;

                createString = "create table " + tableName + " (id int(1) AUTO_INCREMENT primary key";

                foreach (DataRow dr in excelTable.Rows)
                {
                    for (i = 0; i < dr.ItemArray.Length; i++)
                    {
                        createString += ", " + dr.ItemArray[i] + defaultTableItem;
                    }
                    createString += " ) ENGINE = MYISAM CHARSET=utf8";
                    break;
                }

                return i;
            }
            catch (Exception ex)
            {
                Console.WriteLine("getDatabaseCreateStringFromExcel() failed in get create string for " + tableName + "! " + ex);
                return -1;
            }
        }


        //fileName   : path and file name of the excel file 
        //itemLength: we set all item to char type, and itemLength is the length of all column items
        //addRecordFlag: indicating whether we need to add more record into this database on creation
        //            0: only generate database title, no records
        //            1: besides title, we still need to add more records to this table from excel file
        public static int createDataTableFromExcel(string databaseName, string tableName, string fileName, int itemLength, int addRecordFlag)
        {
            int i;
            int ret;
            string path;
            string defaultTableItem = " varchar(" + itemLength + ")";
            string createString;
            DataTable excelTable;

            ret = 1;
            try
            {
                if (fileName == null)
                    return -1;

                path = fileName;
                excelTable = readExcelToDataTable(path, EXCEL_FIRSTLINE_TITLE);
                if (excelTable == null)
                    return -1;

                createString = "create table " + tableName + " (id int(1) AUTO_INCREMENT primary key";

                foreach (DataRow dr in excelTable.Rows)
                {
                    for (i = 0; i < dr.ItemArray.Length; i++)
                    {
                        createString += ", " + dr.ItemArray[i] + defaultTableItem;
                    }
                    createString += " ) ENGINE = MYISAM CHARSET=utf8";
                    break;
                }

                MySqlConnection myConnection = new MySqlConnection("database = " + databaseName + "; " + connectionString);
                MySqlCommand cmd = new MySqlCommand(createString, myConnection);
                myConnection.Open();
                cmd.ExecuteNonQuery();
                myConnection.Close();

                if (addRecordFlag == 1)
                {
                    ret = writeExcelTableToDatabase(excelTable, databaseName, tableName);
                    return ret;
                }
                else
                    return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("database " + databaseName + "Table " + tableName + " generating fail:" + ex);
                return ret;
            }
        }

        //read data from excel into standard data table class
        //input:  fileUrl: file path + file name
        //      firstLine: EXCEL_FIRSTLINE_DATA   -- the first line of excel file is data
        //                 EXCEL_FIRSTLINE_TITLE  -- the first line of excel file is title, should not be put in database  
        //return: DataTable
        public static DataTable readExcelToDataTable(string fileUrl, int firstLine)
        {
            string cmdText;

            //            const string cmdText = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0};Extended Properties='Excel 8.0;IMEX=1';";
            //suport both xls and xlsx, HDR=Yes means first line is titel not data
            if (firstLine == EXCEL_FIRSTLINE_DATA)
                cmdText = "Provider=Microsoft.Ace.OleDb.12.0;Data Source={0};Extended Properties='Excel 12.0; HDR=No; IMEX=1'";
            else //if(firstLine == EXCEL_FIRSTLINE_TITLE)
                cmdText = "Provider=Microsoft.Ace.OleDb.12.0;Data Source={0};Extended Properties='Excel 12.0; HDR=Yes; IMEX=1'";

            DataTable dt = null;
            OleDbDataAdapter da;
            OleDbConnection conn;

            conn = new OleDbConnection(string.Format(cmdText, fileUrl));
            try
            {
                if (conn.State == ConnectionState.Broken || conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }
                System.Data.DataTable schemaTable = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);

                string sheetName = schemaTable.Rows[0]["TABLE_NAME"].ToString().Trim();

                string strSql = "select * from [" + sheetName + "]";
                da = new OleDbDataAdapter(strSql, conn);
                DataSet ds = new DataSet();
                da.Fill(ds);
                dt = ds.Tables[0];
                da.Dispose();

                return dt;
            }
            catch (Exception ex)
            {
                Console.WriteLine("readExcelToDataTable() fail for :" + fileUrl + "! " + ex);
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
            return null;
        }

        //internal use
        static int writeExcelTableToDatabase(DataTable dTable, string databaseName, string tableName)
        {
            int i;
            int value;
            int row;
            int len;
            int ret;
            int flag;
            string str;
            string insertString;
            string commandText;
            string[] strArray;
            MySqlConnection myConnection;

            ret = 0;
            row = 0;
            insertString = null;
            strArray = null;
            try
            {
                foreach (DataRow dr in dTable.Rows)
                {
                    if (row == 0)
                    {
                        strArray = new string[dr.ItemArray.Length];
                        insertString = " value(@id";

                        for(i = 0; i < dr.ItemArray.Length; i++)
                        {
                            insertString += ", @" + dr.ItemArray[i];
                            strArray[i] = "@" + dr.ItemArray[i].ToString();
                        }
                        insertString += ")";
                        row = 1;
                        continue;
                    }
                    len = dr.ItemArray.Length + 1; //excel column number plus 1 for ID at the beginning

                    commandText = "insert into `" + tableName + "`" + insertString;

                    MySqlParameter[] param = new MySqlParameter[len];

                    param[0] = new MySqlParameter("@id", 0);
                    for (i = 1; i < len; i++)
                    {
                        if (dr[i - 1] == null)
                            param[i] = new MySqlParameter(strArray[i - 1].Trim(), "");
                        else
                        {
                            str = dr[i - 1].ToString();
                            value = 0;
                            flag = toolClass.isDigitalNum(str);
                            if(flag == 1)
                            {
                                value = Convert.ToInt32(dr[i - 1].ToString().Trim());
                            }
                            if (value > 42000 && value < 44000)
                            {
                                str = DateTime.FromOADate(value).ToString("yyyy-MM-dd");
                                param[i] = new MySqlParameter(strArray[i - 1].Trim(), str);
                            }
                            else
                            {
                                param[i] = new MySqlParameter(strArray[i - 1].Trim(), dr[i - 1].ToString().Trim());
                            }
                        }
                    }

                    myConnection = new MySqlConnection("database = " + databaseName + "; " + connectionString);
                    myConnection.Open();
                    MySqlCommand myCommand = myConnection.CreateCommand();
                    myCommand.CommandText = commandText;
                    myCommand.Parameters.AddRange(param);
                    myCommand.ExecuteNonQuery();
                    ret = (int)myCommand.LastInsertedId;

                    myConnection.Close();
                }
                return ret;
            }
            catch (Exception ex)
            {
                Console.WriteLine("writeExcelTableToDatabase(" + databaseName + ", " + tableName + ") failed! " + ex);
                return -1;
            }
        }


        //get all records that satisfy the input condition
        //databaseName: database name
        //commandText: condition, some examples below
        //"where workerID = 31085", worker ID is 31085, return only one record
        //"where id = 1", the index in table is 1, return only one record 
        //"where produceTime > 2017-12-08 12:30:03 and produceTime <= 2018-01-06 21:23:03 order by DESC", products between 2017/12/8 to 2018/1/6, return multiple records
        //return: dataArray[,], dataArray[0,0] contains the first column of the first record, dataArray[3,1] contains the second column of the third record, etc
        //        null, no record satisfied the input condition
        public static string[,] getAllRecordFromTableByCondition(String databaseName, String tableName, string commandText)
        {
            int i;
            int recordIndex;
            int recordNum;
            string[,] dataArray;
            MySqlConnection myConnection;
            MySqlDataReader myReader;

            dataArray = null;
            myConnection = null;
            myReader = null;
            try
            {
                myConnection = new MySqlConnection("database = " + databaseName + "; " + connectionString);
                myConnection.Open();

                MySqlCommand myCommand = myConnection.CreateCommand();

                myCommand.CommandText = "select count(*) from `" + tableName + "` " + commandText;
                recordNum = Convert.ToInt32(myCommand.ExecuteScalar());

                myCommand.CommandText = "select * from `" + tableName + "` " + commandText;
                myReader = myCommand.ExecuteReader();
                if (myReader != null)
                {
                    recordIndex = 0;
                    while (myReader.Read())  
                    {
                        if (recordIndex == 0)
                        {
                            //generate this 2 dimention array to store all the records
                            dataArray = new string[recordNum, myReader.FieldCount];
                        }

                        for (i = 0; i < myReader.FieldCount; i++)
                        {
                            if (!myReader.IsDBNull(i))
                                dataArray[recordIndex, i] = myReader.GetString(i);
                            else
                                dataArray[recordIndex, i] = "";
                        }

                        recordIndex++;
                    }
                    myReader.Close();
                }

                myConnection.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(databaseName + "." + tableName + "getAllRecordFromTableByCondition failed, condition is " + commandText + " with exception of: " + ex);
                if (myReader != null)
                    myReader.Close();

                if (myConnection != null)
                    myConnection.Close();
            }

            return dataArray;
        }
    }
}
