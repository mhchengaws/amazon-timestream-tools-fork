﻿using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Amazon.TimestreamWrite;
using Amazon.TimestreamWrite.Model;

namespace TimestreamDotNetSample
{
    public class CrudAndSimpleIngestionExample
    {

        private readonly AmazonTimestreamWriteClient writeClient;

        public CrudAndSimpleIngestionExample(AmazonTimestreamWriteClient writeClient)
        {
            this.writeClient = writeClient;
        }

        public async Task CreateDatabase(string databaseName)
        {
            Console.WriteLine("Creating Database");

            try
            {
                var createDatabaseRequest = new CreateDatabaseRequest
                {
                    DatabaseName = databaseName
                };
                CreateDatabaseResponse response = await writeClient.CreateDatabaseAsync(createDatabaseRequest);
                Console.WriteLine($"Database {databaseName} created");
            }
            catch (ConflictException)
            {
                Console.WriteLine("Database already exists.");
            }
            catch (Exception e)
            {
                Console.WriteLine("Create database failed:" + e.ToString());
            }

        }

        public async Task DescribeDatabase(string databaseName)
        {
            Console.WriteLine("Describing Database");

            try
            {
                var describeDatabaseRequest = new DescribeDatabaseRequest
                {
                    DatabaseName = databaseName
                };
                DescribeDatabaseResponse response = await writeClient.DescribeDatabaseAsync(describeDatabaseRequest);
                Console.WriteLine($"Database {databaseName} has id:{response.Database.Arn}");
            }
            catch (ResourceNotFoundException)
            {
                Console.WriteLine("Database does not exist.");
            }
            catch (Exception e)
            {
                Console.WriteLine("Describe database failed:" + e.ToString());
            }

        }

        public async Task ListDatabases()
        {
            Console.WriteLine("Listing Databases");

            try
            {
                var listDatabasesRequest = new ListDatabasesRequest
                {
                    MaxResults = 5
                };
                ListDatabasesResponse response = await writeClient.ListDatabasesAsync(listDatabasesRequest);
                PrintDatabases(response.Databases);
                var nextToken = response.NextToken;
                while (nextToken != null)
                {
                    listDatabasesRequest.NextToken = nextToken;
                    response = await writeClient.ListDatabasesAsync(listDatabasesRequest);
                    PrintDatabases(response.Databases);
                    nextToken = response.NextToken;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("List database failed:" + e.ToString());
            }

        }

        public async Task UpdateDatabase(String databaseName, String updatedKmsKeyId)
        {
            Console.WriteLine("Updating Database");

            try
            {
                var updateDatabaseRequest = new UpdateDatabaseRequest
                {
                    DatabaseName = databaseName,
                    KmsKeyId = updatedKmsKeyId
                };
                UpdateDatabaseResponse response = await writeClient.UpdateDatabaseAsync(updateDatabaseRequest);
                Console.WriteLine($"Database {databaseName} updated with KmsKeyId {updatedKmsKeyId}");
            }
            catch (ResourceNotFoundException)
            {
                Console.WriteLine("Database does not exist.");
            }
            catch (Exception e)
            {
                Console.WriteLine("Update database failed: " + e.ToString());
            }

        }

        private void PrintDatabases(List<Database> databases)
        {
            foreach (Database database in databases)
                Console.WriteLine($"Database:{database.DatabaseName}");
        }

        public async Task DeleteDatabase(String databaseName)
        {
            Console.WriteLine("Deleting database");
            try
            {
                var deleteDatabaseRequest = new DeleteDatabaseRequest
                {
                    DatabaseName = databaseName
                };
                DeleteDatabaseResponse response = await writeClient.DeleteDatabaseAsync(deleteDatabaseRequest);
                Console.WriteLine($"Database {databaseName} delete request status:{response.HttpStatusCode}");
            }
            catch (ResourceNotFoundException)
            {
                Console.WriteLine($"Database {databaseName} does not exists");
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception while deleting database:" + e.ToString());
            }
        }

        public async Task CreateTable(String databaseName, String tableName)
        {
            Console.WriteLine("Creating Table");

            try
            {
                var createTableRequest = new CreateTableRequest
                {
                    DatabaseName = databaseName,
                    TableName = tableName,
                    RetentionProperties = new RetentionProperties
                    {
                        MagneticStoreRetentionPeriodInDays = Constants.CT_TTL_DAYS,
                        MemoryStoreRetentionPeriodInHours = Constants.HT_TTL_HOURS
                    }
                };
                CreateTableResponse response = await writeClient.CreateTableAsync(createTableRequest);
                Console.WriteLine($"Table {tableName} created");
            }
            catch (ConflictException)
            {
                Console.WriteLine("Table already exists.");
            }
            catch (Exception e)
            {
                Console.WriteLine("Create table failed:" + e.ToString());
            }

        }

        public async Task DescribeTable(String databaseName, String tableName)
        {
            Console.WriteLine("Describing Table");

            try
            {
                var describeTableRequest = new DescribeTableRequest
                {
                    DatabaseName = databaseName,
                    TableName = tableName
                };
                DescribeTableResponse response = await writeClient.DescribeTableAsync(describeTableRequest);
                Console.WriteLine($"Table {tableName} has id:{response.Table.Arn}");
            }
            catch (ResourceNotFoundException)
            {
                Console.WriteLine("Table does not exist.");
            }
            catch (Exception e)
            {
                Console.WriteLine("Describe table failed:" + e.ToString());
            }

        }

        public async Task ListTables(String databaseName)
        {
            Console.WriteLine("Listing Tables");

            try
            {
                var listTablesRequest = new ListTablesRequest
                {
                    MaxResults = 5,
                    DatabaseName = databaseName
                };
                ListTablesResponse response = await writeClient.ListTablesAsync(listTablesRequest);
                PrintTables(response.Tables);
                string nextToken = response.NextToken;
                while (nextToken != null)
                {
                    listTablesRequest.NextToken = nextToken;
                    response = await writeClient.ListTablesAsync(listTablesRequest);
                    PrintTables(response.Tables);
                    nextToken = response.NextToken;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("List table failed:" + e.ToString());
            }

        }

        private void PrintTables(List<Table> tables)
        {
            foreach (Table table in tables)
                Console.WriteLine($"Table: {table.TableName}");
        }

        public async Task UpdateTable(String databaseName, String tableName)
        {
            Console.WriteLine("Updating Table");

            try
            {
                var updateTableRequest = new UpdateTableRequest
                {
                    DatabaseName = databaseName,
                    TableName = tableName,
                    RetentionProperties = new RetentionProperties
                    {
                        MagneticStoreRetentionPeriodInDays = Constants.CT_TTL_DAYS,
                        MemoryStoreRetentionPeriodInHours = Constants.HT_TTL_HOURS
                    }
                };
                UpdateTableResponse response = await writeClient.UpdateTableAsync(updateTableRequest);
                Console.WriteLine($"Table {tableName} updated");
            }
            catch (ResourceNotFoundException)
            {
                Console.WriteLine("Table does not exist.");
            }
            catch (Exception e)
            {
                Console.WriteLine("Update table failed:" + e.ToString());
            }

        }

        public async Task DeleteTable(String databaseName, String tableName)
        {
            Console.WriteLine("Deleting table");
            try
            {
                var deleteTableRequest = new DeleteTableRequest
                {
                    DatabaseName = databaseName,
                    TableName = tableName
                };
                DeleteTableResponse response = await writeClient.DeleteTableAsync(deleteTableRequest);
                Console.WriteLine($"Table {tableName} delete request status: {response.HttpStatusCode}");
            }
            catch (ResourceNotFoundException)
            {
                Console.WriteLine($"Table {tableName} does not exists");
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception while deleting table:" + e.ToString());
            }
        }

        public async Task WriteRecords(String databaseName, String tableName)
        {
            Console.WriteLine("Writing records");

            DateTimeOffset now = DateTimeOffset.UtcNow;
            string currentTimeString = (now.ToUnixTimeMilliseconds()).ToString();

            List<Dimension> dimensions = new List<Dimension>{
                new Dimension { Name = "region", Value = "us-east-1" },
                new Dimension { Name = "az", Value = "az1" },
                new Dimension { Name = "hostname", Value = "host1" }
            };

            var cpuUtilization = new Record
            {
                Dimensions = dimensions,
                MeasureName = "cpu_utilization",
                MeasureValue = "13.6",
                MeasureValueType = MeasureValueType.DOUBLE,
                Time = currentTimeString
            };

            var memoryUtilization = new Record
            {
                Dimensions = dimensions,
                MeasureName = "memory_utilization",
                MeasureValue = "40",
                MeasureValueType = MeasureValueType.DOUBLE,
                Time = currentTimeString
            };


            List<Record> records = new List<Record> {
                cpuUtilization,
                memoryUtilization
            };

            try
            {
                var writeRecordsRequest = new WriteRecordsRequest
                {
                    DatabaseName = databaseName,
                    TableName = tableName,
                    Records = records
                };
                WriteRecordsResponse response = await writeClient.WriteRecordsAsync(writeRecordsRequest);
                Console.WriteLine($"Write records status code: {response.HttpStatusCode.ToString()}");
            }
            catch (RejectedRecordsException e) {
                PrintRejectedRecordsException(e);
            }
            catch (Exception e)
            {
                Console.WriteLine("Write records failure:" + e.ToString());
            }
        }

        public async Task WriteRecordsWithCommonAttributes(String databaseName, String tableName)
        {
            Console.WriteLine("Writing records with common attributes");

            DateTimeOffset now = DateTimeOffset.UtcNow;
            string currentTimeString = (now.ToUnixTimeMilliseconds()).ToString();

            List<Dimension> dimensions = new List<Dimension>{
                new Dimension { Name = "region", Value = "us-east-1" },
                new Dimension { Name = "az", Value = "az1" },
                new Dimension { Name = "hostname", Value = "host1" }
            };

            var commonAttributes = new Record
            {
                Dimensions = dimensions,
                MeasureValueType = MeasureValueType.DOUBLE,
                Time = currentTimeString
            };

            var cpuUtilization = new Record
            {
                MeasureName = "cpu_utilization",
                MeasureValue = "13.6"
            };

            var memoryUtilization = new Record
            {
                MeasureName = "memory_utilization",
                MeasureValue = "40"
            };


            List<Record> records = new List<Record>();
            records.Add(cpuUtilization);
            records.Add(memoryUtilization);

            try
            {
                var writeRecordsRequest = new WriteRecordsRequest
                {
                    DatabaseName = databaseName,
                    TableName = tableName,
                    Records = records,
                    CommonAttributes = commonAttributes
                };
                WriteRecordsResponse response = await writeClient.WriteRecordsAsync(writeRecordsRequest);
                Console.WriteLine($"Write records status code: {response.HttpStatusCode.ToString()}");
            }
            catch (RejectedRecordsException e) {
                PrintRejectedRecordsException(e);
            }
            catch (Exception e)
            {
                Console.WriteLine("Write records failure:" + e.ToString());
            }
        }

        public async Task WriteRecordsWithUpsert(String databaseName, String tableName)
            {
                Console.WriteLine("Writing records with upsert");

                DateTimeOffset now = DateTimeOffset.UtcNow;
                string currentTimeString = (now.ToUnixTimeMilliseconds()).ToString();
                // To achieve upsert (last writer wins) semantic, one example is to use current time as the version if you are writing directly from the data source
                long version = now.ToUnixTimeMilliseconds();

                List<Dimension> dimensions = new List<Dimension>{
                    new Dimension { Name = "region", Value = "us-east-1" },
                    new Dimension { Name = "az", Value = "az1" },
                    new Dimension { Name = "hostname", Value = "host1" }
                };

                var commonAttributes = new Record
                {
                    Dimensions = dimensions,
                    MeasureValueType = MeasureValueType.DOUBLE,
                    Time = currentTimeString,
                    Version = version
                };

                var cpuUtilization = new Record
                {
                    MeasureName = "cpu_utilization",
                    MeasureValue = "13.6"
                };

                var memoryUtilization = new Record
                {
                    MeasureName = "memory_utilization",
                    MeasureValue = "40"
                };


                List<Record> records = new List<Record>();
                records.Add(cpuUtilization);
                records.Add(memoryUtilization);

                // write records for first time
                try
                {
                    var writeRecordsRequest = new WriteRecordsRequest
                    {
                        DatabaseName = databaseName,
                        TableName = tableName,
                        Records = records,
                        CommonAttributes = commonAttributes
                    };
                    WriteRecordsResponse response = await writeClient.WriteRecordsAsync(writeRecordsRequest);
                    Console.WriteLine($"WriteRecords Status for first time: {response.HttpStatusCode.ToString()}");
                }
                catch (RejectedRecordsException e) {
                    PrintRejectedRecordsException(e);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Write records failure:" + e.ToString());
                }

                // Successfully retry same writeRecordsRequest with same records and versions, because writeRecords API is idempotent.
                try
                {
                    var writeRecordsRequest = new WriteRecordsRequest
                    {
                        DatabaseName = databaseName,
                        TableName = tableName,
                        Records = records,
                        CommonAttributes = commonAttributes
                    };
                    WriteRecordsResponse response = await writeClient.WriteRecordsAsync(writeRecordsRequest);
                    Console.WriteLine($"WriteRecords Status for retry: {response.HttpStatusCode.ToString()}");
                }
                catch (RejectedRecordsException e) {
                    PrintRejectedRecordsException(e);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Write records failure:" + e.ToString());
                }

                // upsert with lower version, this would fail because a higher version is required to update the measure value.
                version--;
                Type recordType = typeof(Record);
                recordType.GetProperty("Version").SetValue(commonAttributes, version);
                recordType.GetProperty("MeasureValue").SetValue(cpuUtilization, "14.6");
                recordType.GetProperty("MeasureValue").SetValue(memoryUtilization, "50");

                List<Record> upsertedRecords = new List<Record> {
                    cpuUtilization,
                    memoryUtilization
                };

                try
                {
                    var writeRecordsUpsertRequest = new WriteRecordsRequest
                    {
                        DatabaseName = databaseName,
                        TableName = tableName,
                        Records = upsertedRecords,
                        CommonAttributes = commonAttributes
                    };
                    WriteRecordsResponse upsertResponse = await writeClient.WriteRecordsAsync(writeRecordsUpsertRequest);
                    Console.WriteLine($"WriteRecords Status for upsert with lower version: {upsertResponse.HttpStatusCode.ToString()}");
                }
                catch (RejectedRecordsException e) {
                    PrintRejectedRecordsException(e);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Write records failure:" + e.ToString());
                }

                // upsert with higher version as new data in generated
                now = DateTimeOffset.UtcNow;
                version = now.ToUnixTimeMilliseconds();
                recordType.GetProperty("Version").SetValue(commonAttributes, version);

                try
                {
                    var writeRecordsUpsertRequest = new WriteRecordsRequest
                    {
                        DatabaseName = databaseName,
                        TableName = tableName,
                        Records = upsertedRecords,
                        CommonAttributes = commonAttributes
                    };
                    WriteRecordsResponse upsertResponse = await writeClient.WriteRecordsAsync(writeRecordsUpsertRequest);
                    Console.WriteLine($"WriteRecords Status for upsert with higher version:  {upsertResponse.HttpStatusCode.ToString()}");
                }
                catch (RejectedRecordsException e) {
                    PrintRejectedRecordsException(e);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Write records failure:" + e.ToString());
                }
            }

        private void PrintRejectedRecordsException(RejectedRecordsException e)
        {
            Console.WriteLine("RejectedRecordsException:" + e.ToString());
            foreach (RejectedRecord rr in e.RejectedRecords) {
                Console.WriteLine("RecordIndex " + rr.RecordIndex + " : " + rr.Reason);
                long? existingVersion = rr.ExistingVersion;
                if (existingVersion != null) {
                    Console.WriteLine("Rejected record existing version: " + existingVersion);
                }
            }
        }
    }
}
