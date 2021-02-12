using System;
using System.Data.SQLite;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using CsvHelper;
using System.Collections;
using System.Globalization;




namespace FactorySystemsCodingChallenge
{
    public class CodingChallenge
    {
        
        private static StringBuilder csvHero = new StringBuilder();
       // private static String location; loading for ARGS automation. Discussion point***
        private static ArrayList valList = new ArrayList();
        private static SQLiteConnection sql_conn;
        static void Main(string[] args)
        {

            //Here we collect the address of the database from the user and use it to create the connection
            sql_conn = CreateConnection(databasePrompt());
            //calling the method to create the CSV and write the rows for each test
            validateTest(sql_conn);
            writeCSV(valList);

        }

        //this method prompts the user to enter the location of a database to pass through the application
        static String databasePrompt()
        {
            String n = "";
            Console.WriteLine("Please enter the location of the database you wish to use");
            n = "Data Source=" + Console.ReadLine() + "; Version=3;New=True; Compress=True;";
            Console.WriteLine(n);
            Console.WriteLine("Address accepted");




            return n;
        }



        //this method create the connection by using the prompted location as a parameter.
        static SQLiteConnection CreateConnection(String databaselocal)
        {
            SQLiteConnection sql_conn;

            sql_conn = new SQLiteConnection(databaselocal);
            try
            {
                sql_conn.Open();
            }
            catch (Exception ex)
            {


            }
            return sql_conn;

        }
        //this method gathers the minimum height and location of the records and displays it.
        static String[,] minData(SQLiteConnection conn, int i)
        {
            //creating a 2d array to store our two values
            String[,] minArray = new String[1, 2];
            //setting up our statements to collect our query resultset
            SQLiteDataReader sqlite_datareader;
            SQLiteCommand sqlite_cmd;
            sqlite_cmd = conn.CreateCommand();
            sqlite_cmd.CommandText = "SELECT Min(height),PlaneID FROM Measurements m JOIN Tests t WHERE m.test_uid =" + i + ";";
            sqlite_datareader = sqlite_cmd.ExecuteReader();
            while (sqlite_datareader.Read())
            {
                string myreader = (" " + sqlite_datareader.GetDecimal(0) + " " + sqlite_datareader.GetString(1));
                //storing our results into our array
                minArray[0, 0] = "" + sqlite_datareader.GetDecimal(0);
                minArray[0, 1] = sqlite_datareader.GetString(1);



            }
            return minArray;
        }

        //this method gathers the maximum height and location of the records and diplays it. 
        static String[,] maxData(SQLiteConnection conn, int i)
        {   //creating a 2d array to store our two values
            String[,] maxArray = new String[1, 2];
            //setting up the statements to retrieve our query resultset
            SQLiteDataReader sqlite_datareader;
            SQLiteCommand sqlite_cmd;
            sqlite_cmd = conn.CreateCommand();
            sqlite_cmd.CommandText = "SELECT Max(height),PlaneID FROM Measurements m JOIN Tests t WHERE m.test_uid = " + i + ";";

            sqlite_datareader = sqlite_cmd.ExecuteReader();
            while (sqlite_datareader.Read())
            {
                string myreader = (" " + sqlite_datareader.GetDecimal(0) + " " + sqlite_datareader.GetString(1));
                //storing our results into the array
                maxArray[0, 0] = "" + sqlite_datareader.GetDecimal(0);
                maxArray[0, 1] = sqlite_datareader.GetString(1);

            }
            return maxArray;
        }
        //This method gathers the mean data of the tests
        static decimal meanData(SQLiteConnection conn, int i)
        {
            //setting up the statements to retreive our mean data 
            SQLiteDataReader sqlite_datareader;
            SQLiteCommand sqlite_cmd;
            sqlite_cmd = conn.CreateCommand();
            sqlite_cmd.CommandText = "SELECT avg(height)FROM Measurements WHERE test_uid=" + i + ";";
            //storing our result set in a decimal and returning it
            sqlite_datareader = sqlite_cmd.ExecuteReader();
            decimal d = 0;
            while (sqlite_datareader.Read())
            {
                string myreader = (" " + sqlite_datareader.GetDecimal(0));
                d = sqlite_datareader.GetDecimal(0);


            }
            return d;
        }
        //this method collects the range data and returns it for our purposes
        static decimal rangeData(SQLiteConnection conn, int i)
        {
            //decimal d is our storage variable
            decimal d = 0;

            //setting up the statements to retrieve our range data
            SQLiteDataReader sqlite_datareader;
            SQLiteCommand sqlite_cmd;
            sqlite_cmd = conn.CreateCommand();
            sqlite_cmd.CommandText = "SELECT max(height)-min(height) FROM Measurements WHERE test_uid=" + i + ";";

            sqlite_datareader = sqlite_cmd.ExecuteReader();
            while (sqlite_datareader.Read())
            {
                string myreader = (" " + sqlite_datareader.GetDecimal(0));


                d = sqlite_datareader.GetDecimal(0);
            }

            return d;
        }
        //this method calculates the average roughness 
        static decimal raData(SQLiteConnection conn, int i)
        {
            SQLiteDataReader sqlite_datareader;

            SQLiteCommand sqlite_cmd;

            sqlite_cmd = conn.CreateCommand();

            sqlite_cmd.CommandText = "SELECT * FROM Measurements WHERE test_uid=" + i + ";";

            //our variables needed for the calculation are stored below
            decimal total = 0;
            int num = 0;
            decimal d = 0;

            //total is appended with the absolute value of each read divided by the total number of readings
            sqlite_datareader = sqlite_cmd.ExecuteReader();
            while (sqlite_datareader.Read())
            {
                total += Math.Abs(sqlite_datareader.GetDecimal(4));
                num++;
                d = total / num;
            }
            return d;
        }
        //this method gathers the Root Mean Square Roughness
        static double rmsData(SQLiteConnection conn, int i)
        {

            SQLiteDataReader sqlite_datareader;
            SQLiteCommand sqlite_cmd;
            sqlite_cmd = conn.CreateCommand();
            sqlite_cmd.CommandText = "SELECT * FROM Measurements WHERE test_uid=" + i + ";";
            //selecting all of the table
            double total = 0;
            int num = 0;

            //only acting on the 4 member of the entries we calculate using the formula
            sqlite_datareader = sqlite_cmd.ExecuteReader();
            while (sqlite_datareader.Read())
            {
                total += Math.Pow((double)sqlite_datareader.GetDecimal(4), 2.0);
                num++;

            }
            double d = Math.Pow((double)(total / num), 0.5);
            return d;
        }

        //this method creates the min entry in the CSV File
        static void writeMinCSV(int i)
        {
            String[,] minOutput = minData(sql_conn, i);
            int ilength = minOutput.GetLength(0);
            for (int x = 0; x < ilength; x++)
            {
                string strSeperator = ",";
                csvHero.Append("Minimum height is = ");
                csvHero.Append(string.Join(strSeperator, minOutput[x, x]));
                csvHero.Append(" location = ");
                csvHero.AppendLine(string.Join(strSeperator, minOutput[x, x + 1]));

            }



        }

        //this method creates the max entry in the CSV File
        static void writeMaxCSV(int i)
        {



            string strSeperator = ",";
            String[,] maxOutput = maxData(sql_conn, i);

            int ilength = maxOutput.GetLength(0);

            for (int x = 0; x < ilength; x++)
            {
                csvHero.Append("Maximum height is = ");
                csvHero.Append(string.Join(strSeperator, maxOutput[x, x]));
                csvHero.Append(" location = ");
                csvHero.AppendLine(maxOutput[x, x + 1]);


            }



        }
        //this method creates the mean entry in the CSV File
        static void writeMeanCSV(int i)
        {




            string strSeperator = ",";
            Decimal meanOutput = meanData(sql_conn, i);
            csvHero.Append("Mean height is = ");
            csvHero.AppendLine(string.Join(strSeperator, meanOutput));




        }
        //this method creates the range entry in the CSV File
        static void writeRangeCSV(int i)
        {






            Decimal rangeOutput = rangeData(sql_conn, i);


            string strSeperator = ",";
            csvHero.Append("Height range is = ");
            csvHero.AppendLine(string.Join(strSeperator, rangeOutput));



        }
        //this method creates the average roughness entry in the CSV File
        static void writeRACSV(int i)     
        {
            Decimal raOutput = raData(sql_conn, i);


            string strSeperator = ",";
            csvHero.Append("Average Roughness is = ");
            csvHero.AppendLine(string.Join(strSeperator, raOutput));



        }

        //this method creates the root mean square roughness entry in the CSV File
        static StringBuilder writeRMSCSV(int i)
        {
            double rmsOutput = rmsData(sql_conn, i);

            string strSeperator = ",";

            csvHero.Append("Root Mean Square Roughness is = ");
            csvHero.AppendLine(string.Join(strSeperator, rmsOutput));



            return csvHero;
        }
        //this method is included to clean up the method calls in the main method
        //this method also allows for formatting. 
        static void writeCSV(ArrayList alist)
        {
            string strFilePath = @"C:\CSV\FactorySystemsReport.csv";



            for (int x = 0; x < alist.Count; x++)
            {

                csvHero.AppendLine("TestNumber =" + Convert.ToInt32(alist[x]));

                writeMinCSV(Convert.ToInt32(alist[x]));

                writeMaxCSV(Convert.ToInt32(alist[x]));

                writeMeanCSV(Convert.ToInt32(alist[x]));

                writeRangeCSV(Convert.ToInt32(alist[x]));

                writeRACSV(Convert.ToInt32(alist[x]));

                writeRMSCSV(Convert.ToInt32(alist[x]));

                File.WriteAllText(strFilePath, csvHero.ToString());

            }

            Console.WriteLine("The CSV has been generated at C:\\CSV\\FactorySystemsReport.csv");
        }
        //this method collects  the entries supplied by the database which have 1000 measurements in order exclude any tests which do not have 1000 measurements. 
        //it achieves this by collecting the test_uids that meet the criteria and then serving them to an arraylist. This abstracts tests which do not pass criteria.
        static ArrayList validateTest(SQLiteConnection conn)
        {

            int i = 0;
            SQLiteDataReader sqlite_datareader;
            SQLiteCommand sqlite_cmd;
            sqlite_cmd = conn.CreateCommand();

            sqlite_cmd.CommandText = "select test_uid from Measurements Group By test_uid having COUNT(measurement_uid)=1000;";

            sqlite_datareader = sqlite_cmd.ExecuteReader();


            while (sqlite_datareader.Read())
            {


                valList.Add(sqlite_datareader.GetInt32(0));
                
                i++;

            }





            return valList;



        }
    }
}