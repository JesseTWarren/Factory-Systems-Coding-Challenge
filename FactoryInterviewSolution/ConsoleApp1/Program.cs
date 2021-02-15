using System;
using System.Data;
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

/*Jesse Warren 
 *Factory Systems Coding Challenge 
 */

namespace FactorySystemsCodingChallenge
{
    public class CodingChallenge
    {
        static DataTable databaseInput = new DataTable();

        static DataTable databaseOutput = new DataTable();

        static StringBuilder sb = new StringBuilder();

        static Int64[] valList;
        static double maxVar;
        static double minVar;
        static double avgVar;
        static double rangeVar;




        private static SQLiteConnection sql_conn;
        static void Main(string[] args)
        {
            //Here we collect the address of the database from the user and validate it.
            sql_conn = databasePrompt();
            //calling the connection to write the rows for each test and exclude invalid tests
            validateTest(sql_conn, databaseInput);
            //getting an list of the test ids for iteration purposes
            getValidatedTestID(databaseInput);
            //setting up and filling the reports datatable
            createReportTable();
            //creating a folder for and printing the datatable to CSV file and notifying the user of it's location
            printToCSV(databaseOutput);
            //A send off message
            Console.WriteLine("Thank you for using the Summary Generator");
            Console.ReadLine();

        }

        //this method prompts the user to enter the location of a database to pass through the application and returns the connection
        static SQLiteConnection databasePrompt()
        {   //using an if statement to validate database location
            bool val1 = true;
            String n = "";
            String validation = "";
            Console.WriteLine("Please enter the location of the database you wish to use");
            do
            {
                validation = Console.ReadLine();
                if (!File.Exists(validation))
                {
                    Console.WriteLine("Please enter a valid address");
                }
                if (File.Exists(validation))
                { break; }
            }
            while (val1 == true);
            { }
            n = "Data Source=" + validation + "; Version=3;New=True; Compress=True;";
            Console.WriteLine(n);
            Console.WriteLine("Address accepted");
            SQLiteConnection sql_conn;
            sql_conn = new SQLiteConnection(n);
            return sql_conn;

        }
        //this method opens the connection and gathers the entire valid test base in one query then the method stores the results in a datatable then closes the connection
        static DataTable validateTest(SQLiteConnection conn, DataTable dim)
        {
            ArrayList valNum = new ArrayList();

            var tableAdapter = new SQLiteDataAdapter("select m.test_uid, height, x, y, measurement_uid, sTime, PlaneID, Operator from Measurements m join Tests t on t.test_uid=m.test_uid Group by measurement_uid having COUNT(measurement_uid)=COUNT(1000)", conn);

            conn.Open();

            tableAdapter.Fill(dim);

            conn.Close();

            return dim;
        }

        //this method evalutes the datatable in order to grab the test_uid field and store values in an array.
        static Int64[] getValidatedTestID(DataTable dim)
        {
            DataView view = new DataView(dim);
            DataTable distinctValues = view.ToTable(true, "test_uid");
            DataRow[] conValue = new DataRow[distinctValues.Rows.Count];
            distinctValues.Rows.CopyTo(conValue, 0);

            valList = (from row in conValue.AsEnumerable()
                       select row.Field<Int64>("test_uid")).ToArray();

            return valList;
        }
        //this method sets up our DataTable for databaseOutput
        static DataTable createReportTable()
        {
            //setting up the columns for the table
            databaseOutput.Clear();
            databaseOutput.Columns.Add("Test ID");
            databaseOutput.Columns.Add("Min Height");
            databaseOutput.Columns.Add("Min Location");
            databaseOutput.Columns.Add("Min Time");
            databaseOutput.Columns.Add("Min Operator");
            databaseOutput.Columns.Add("Max Height");
            databaseOutput.Columns.Add("Max Location");
            databaseOutput.Columns.Add("Max Time");
            databaseOutput.Columns.Add("Max Operator");
            databaseOutput.Columns.Add("Mean Height");
            databaseOutput.Columns.Add("Height Range");
            databaseOutput.Columns.Add("Average Roughness");
            databaseOutput.Columns.Add("Root Mean Square Roughness");
            //this loop applies values lifted from calculations and applies them to the datatable. Values are plucked using parameterized values from the test_uid list
            DataRow row;
            for (int x = 0; x < valList.Length; x++)
            {
                row = databaseOutput.NewRow();
                row["Test ID"] = valList[x];
                row["Min Height"] = calculateMin((int)valList[x]);
                row["Min Location"] = calcLocate((double)calculateMin((int)valList[x]));
                row["Min Time"] = calcTime((double)calculateMin((int)valList[x]));
                row["Min Operator"] = calcOperator((double)calculateMin((int)valList[x]));
                row["Max Height"] = calculateMax((int)valList[x]);
                row["Max Location"] = calcLocate((double)calculateMax((int)valList[x]));
                row["Max Time"] = calcTime((double)calculateMax((int)valList[x]));
                row["Max Operator"] = calcOperator((double)calculateMax((int)valList[x]));
                row["Mean Height"] = calculateMean((int)valList[x]);
                row["Height Range"] = calculateRange(ref maxVar, ref minVar);
                row["Average Roughness"] = calculateAvgRough((int)valList[x]);
                row["Root Mean Square Roughness"] = calculcateRootMeanSquareRoughness((int)valList[x]);
                //adding our data row to the table
                databaseOutput.Rows.Add(row);

            }
            return databaseOutput;
        }

        //this method prints our datatable in CSV format
        static void printToCSV(DataTable dim)
        {//folder location
            string folderPath = @"C:\CSV";
            //file location
            string strFilePath = @"C:\CSV\FactorySystemsSummary.csv";
            //creating our column names to print
            string[] columnNames = dim.Columns.Cast<DataColumn>().
                                              Select(column => column.ColumnName).
                                              ToArray();

            sb.AppendLine(string.Join(",", columnNames));
            //appending our database to our stringbuilder
            foreach (DataRow row in dim.Rows)
            {
                string[] fields = row.ItemArray.Select(field => field.ToString()).
                                                ToArray();
                sb.AppendLine(string.Join(",", fields));
            }

            //if the directory doesn't exist, create it
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            //writing the datatable and giving notification to the user.
            File.WriteAllText(strFilePath, sb.ToString());
            Console.WriteLine("CSV Generated at " + strFilePath);
        }

        //this method calculates minimum height by using a compute function alongside the parameterized test uid
        static double calculateMin(int test_uid)
        {

            double min = (double)databaseInput.Compute("Min(height)", "test_uid=" + test_uid);//come back for this
            minVar = min;
            return minVar;
        }
        //this method calculates max height by using a compute function alongside the parameterized test uid
        static double calculateMax(int test_uid)
        {
            double max = (double)databaseInput.Compute("Max(height)", "test_uid=" + test_uid);
            maxVar = max;
            return maxVar;
        }
        //this method finds the location using a parameterized datatable query
        static string calcLocate(double height)
        {
            string calcLoc = (from DataRow dr in databaseInput.Rows
                              where (double)dr["height"] == height
                              select (string)dr["PlaneID"]).SingleOrDefault();

            return calcLoc;
        }
        //this method finds the sTime field using a parameterized datatable query
        static DateTime? calcTime(double height)
        {
            DateTime? sTime = (from DataRow dr in databaseInput.Rows
                               where (double)dr["height"] == height
                               select (DateTime?)dr["sTime"]).SingleOrDefault();

            return sTime;

        }
        //this method finds the operator for the test using a parameterized query. this query has to explicitly handle null values.
        static string calcOperator(double height)
        {
            string driver = "";

            if (databaseInput != null && databaseInput.Rows.Count > 0)
            {
                foreach (DataRow dr in databaseInput.Rows)
                {
                    if (dr.Field<double>("height") == height)
                    {
                        if (string.IsNullOrWhiteSpace(dr.Field<string>("Operator")))
                        {
                            driver = "No Operator";
                            return driver;
                        }
                        else
                        {
                            driver = dr.Field<string>("Operator");
                            return driver;
                        }
                    }
                }
            }
            return driver;
        }
        //this method calcualates the mean using a datatable compute function
        static double calculateMean(int test_uid)
        {
            double avg = (double)databaseInput.Compute("AVG(height)", "test_uid=" + test_uid);
            avgVar = avg;
            return avgVar;
        }
        //this methid calculates the range using the minVar and maxVar variables updated during the creation loop
        static double calculateRange(ref double x, ref double m)
        {
            double range = x - m;
            rangeVar = range;
            return rangeVar;
        }
        //this method fetches a column to count up from in the table for the number of sums, then I use a linq statement to evaluate for the sum of the absolute values
        //this is evaluated against the number of values to achieve average roughness calculation
        static double calculateAvgRough(int test_uid)
        {
            int num = 0;
            double avg = calculateMean(test_uid);
            double heightTool = 0; ;

            foreach (DataRow dr in databaseInput.Rows)
            {
                if (dr.Field<Int64>("test_uid") == test_uid)
                {
                    heightTool+= Math.Abs(dr.Field<double>("height")-avg);
                    num++;
                }
            }
            
           double avgRoughVar = heightTool/ num;                

            

           return avgRoughVar;

        }


        //this method calculates RMSR by fetching the number of the sums and then evaluating the total sum to the power of two 
        //then placing the total divided by the sum to another power.
        static double calculcateRootMeanSquareRoughness(int test_uid)
        {
            
            double rootMeanSquareVar;
            double avg = calculateMean(test_uid);
            double heightTool = 0;
            int num = 0;

            foreach(DataRow dr in databaseInput.Rows)
            {
                if (dr.Field<Int64>("test_uid") == test_uid)
                {
                    heightTool+=Math.Pow(dr.Field<double>("height")-avg, 2.0);
                    num++;
                }
            }
            rootMeanSquareVar = Math.Sqrt(heightTool / num);

            return rootMeanSquareVar;

        }
    }
}
    

    


















  




