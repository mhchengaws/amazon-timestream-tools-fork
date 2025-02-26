import { constants } from "./constants.js";
import {
    CreateDatabaseCommand, CreateTableCommand, DeleteDatabaseCommand, DeleteTableCommand,
    DescribeDatabaseCommand, DescribeTableCommand,
    ListDatabasesCommand, ListTablesCommand,
    UpdateDatabaseCommand, UpdateTableCommand, WriteRecordsCommand
} from "@aws-sdk/client-timestream-write";

export async function createDatabase(writeClient) {
    console.log("Creating Database");
    const params = new CreateDatabaseCommand({
        DatabaseName: constants.DATABASE_NAME
    });

    await writeClient.send(params).then(
        (data) => {
            console.log(`Database ${data.Database.DatabaseName} created successfully`);
        },
        (err) => {
            if (err.code === 'ConflictException') {
                console.log(`Database ${params.DatabaseName} already exists. Skipping creation.`);
            } else {
                console.log("Error creating database", err);
            }
        }
    );
}

export async function describeDatabase (writeClient) {
    console.log("Describing Database");
    const params = new DescribeDatabaseCommand({
        DatabaseName: constants.DATABASE_NAME
    });

    await writeClient.send(params).then(
        (data) => {
            console.log(`Database ${data.Database.DatabaseName} has id ${data.Database.Arn}`);
        },
        (err) => {
            if (err.code === 'ResourceNotFoundException') {
                console.log("Database doesn't exists.");
            } else {
                console.log("Describe database failed.", err);
                throw err;
            }
        }
    );
}

export async function updateDatabase(updatedKmsKeyId) {

    if (updatedKmsKeyId === undefined) {
        console.log("Skipping UpdateDatabase; KmsKeyId was not given");
        return;
    }
    console.log("Updating Database");
    const params = new UpdateDatabaseCommand({
        DatabaseName: constants.DATABASE_NAME,
        KmsKeyId: updatedKmsKeyId
    });

    await writeClient.send(params).then(
        (data) => {
            console.log(`Database ${data.Database.DatabaseName} updated kmsKeyId to ${updatedKmsKeyId}`);
        },
        (err) => {
            if (err.code === 'ResourceNotFoundException') {
                console.log("Database doesn't exist.");
            } else {
                console.log("Update database failed.", err);
            }
        }
    );
}

export async function listDatabases(writeClient) {
    console.log("Listing databases:");
    const databases = await getDatabasesList(null, writeClient);
    databases.forEach(function(database){
        console.log(database.DatabaseName);
    });
}

function getDatabasesList(nextToken, writeClient, databases = []) {
    var params = new ListDatabasesCommand({
        MaxResults: 15
    });

    if(nextToken) {
        params.NextToken = nextToken;
    }

    return writeClient.send(params).then(
            (data) => {
                databases.push.apply(databases, data.Databases);
                if (data.NextToken) {
                    return getDatabasesList(data.NextToken, databases);
                } else {
                    return databases;
                }
            },
            (err) => {
                console.log("Error while listing databases", err);
            });
}

export async function createTable(writeClient) {
    console.log("Creating Table");
    const params = new CreateTableCommand({
        DatabaseName: constants.DATABASE_NAME,
        TableName: constants.TABLE_NAME,
        RetentionProperties: {
            MemoryStoreRetentionPeriodInHours: constants.HT_TTL_HOURS,
            MagneticStoreRetentionPeriodInDays: constants.CT_TTL_DAYS
        }
    });

    await writeClient.send(params).then(
        (data) => {
            console.log(`Table ${data.Table.TableName} created successfully`);
        },
        (err) => {
            if (err.code === 'ConflictException') {
                console.log(`Table ${params.TableName} already exists on db ${params.DatabaseName}. Skipping creation.`);
            } else {
                console.log("Error creating table. ", err);
                throw err;
            }
        }
    );
}

export async function updateTable(writeClient) {
    console.log("Updating Table");
    const params = new UpdateTableCommand({
        DatabaseName: constants.DATABASE_NAME,
        TableName: constants.TABLE_NAME,
        RetentionProperties: {
            MemoryStoreRetentionPeriodInHours: constants.HT_TTL_HOURS,
            MagneticStoreRetentionPeriodInDays: constants.CT_TTL_DAYS
        }
    });

    await writeClient.send(params).then(
        (data) => {
            console.log("Table updated")
        },
        (err) => {
            console.log("Error updating table. ", err);
            throw err;
        }
    );
}

export async function describeTable(writeClient) {
    console.log("Describing Table");
    const params = new DescribeTableCommand({
        DatabaseName: constants.DATABASE_NAME,
        TableName: constants.TABLE_NAME
    });

    await writeClient.send(params).then(
        (data) => {
            console.log(`Table ${data.Table.TableName} has id ${data.Table.Arn}`);
        },
        (err) => {
            if (err.code === 'ResourceNotFoundException') {
                console.log("Table or Database doesn't exists.");
            } else {
                console.log("Describe table failed.", err);
                throw err;
            }
        }
    );
}

export async function listTables(writeClient) {
    console.log("Listing tables:");
    const tables = await getTablesList(null, writeClient);
    tables.forEach(function(table){
        console.log(table.TableName);
    });
}

function getTablesList(nextToken, writeClient, tables = []) {
    var params = new ListTablesCommand({
        DatabaseName: constants.DATABASE_NAME,
        MaxResults: 15
    });

    if(nextToken) {
        params.NextToken = nextToken;
    }

    return writeClient.send(params).then(
            (data) => {
                tables.push.apply(tables, data.Tables);
                if (data.NextToken) {
                    return getTablesList(data.NextToken, tables);
                } else {
                    return tables;
                }
            },
            (err) => {
                console.log("Error while listing databases", err);
            });
}

export async function writeRecords(writeClient) {
    console.log("Writing records");
    const currentTime = Date.now().toString(); // Unix time in milliseconds

    const dimensions = [
        {'Name': 'region', 'Value': 'us-east-1'},
        {'Name': 'az', 'Value': 'az1'},
        {'Name': 'hostname', 'Value': 'host1'}
    ];

    const cpuUtilization = {
        'Dimensions': dimensions,
        'MeasureName': 'cpu_utilization',
        'MeasureValue': '13.5',
        'MeasureValueType': 'DOUBLE',
        'Time': currentTime.toString()
    };

    const memoryUtilization = {
        'Dimensions': dimensions,
        'MeasureName': 'memory_utilization',
        'MeasureValue': '40',
        'MeasureValueType': 'DOUBLE',
        'Time': currentTime.toString()
    };

    const records = [cpuUtilization, memoryUtilization];

    const params = new WriteRecordsCommand({
        DatabaseName: constants.DATABASE_NAME,
        TableName: constants.TABLE_NAME,
        Records: records
    });

    await writeClient.send(params).then(
        (data) => {
            console.log("Write records successful");
        },
        (err) => {
            console.log("Error writing records:", err);
            if (err.code === 'RejectedRecordsException') {
                printRejectedRecordsException(request);
            }
        }
    );
}

export async function writeRecordsWithCommonAttributes(writeClient) {
    console.log("Writing records with common attributes");
    const currentTime = Date.now().toString(); // Unix time in milliseconds

    const dimensions = [
        {'Name': 'region', 'Value': 'us-east-1'},
        {'Name': 'az', 'Value': 'az1'},
        {'Name': 'hostname', 'Value': 'host1'}
    ];

    const commonAttributes = {
        'Dimensions': dimensions,
        'MeasureValueType': 'DOUBLE',
        'Time': currentTime.toString()
    };

    const cpuUtilization = {
        'MeasureName': 'cpu_utilization',
        'MeasureValue': '13.5'
    };

    const memoryUtilization = {
        'MeasureName': 'memory_utilization',
        'MeasureValue': '40'
    };

    const records = [cpuUtilization, memoryUtilization];

    const params = new WriteRecordsCommand({
        DatabaseName: constants.DATABASE_NAME,
        TableName: constants.TABLE_NAME,
        Records: records,
        CommonAttributes: commonAttributes
    });

    await writeClient.send(params).then(
        (data) => {
            console.log("Write records successful");
        },
        (err) => {
            console.log("Error writing records:", err);
            if (err.code === 'RejectedRecordsException') {
                printRejectedRecordsException(request);
            }
        }
    );
}

export async function writeRecordsWithUpsert(writeClient) {
    console.log("Writing records with upsert");
    const currentTime = Date.now().toString(); // Unix time in milliseconds
    // To achieve upsert (last writer wins) semantic, one example is to use current time as the version if you are writing directly from the data source
    let version = Date.now();

    const dimensions = [
        {'Name': 'region', 'Value': 'us-east-1'},
        {'Name': 'az', 'Value': 'az1'},
        {'Name': 'hostname', 'Value': 'host1'}
    ];

    const commonAttributes = {
        'Dimensions': dimensions,
        'MeasureValueType': 'DOUBLE',
        'Time': currentTime.toString(),
        'Version': version
    };

    const cpuUtilization = {
        'MeasureName': 'cpu_utilization',
        'MeasureValue': '13.5'
    };

    const memoryUtilization = {
        'MeasureName': 'memory_utilization',
        'MeasureValue': '40'
    };

    const records = [cpuUtilization, memoryUtilization];

    const params = new WriteRecordsCommand({
        DatabaseName: constants.DATABASE_NAME,
        TableName: constants.TABLE_NAME,
        Records: records,
        CommonAttributes: commonAttributes
    });

    // write records for first time
    await writeClient.send(params).then(
        (data) => {
            console.log("Write records successful for first time.");
        },
        (err) => {
            console.log("Error writing records:", err);
            if (err.code === 'RejectedRecordsException') {
                printRejectedRecordsException(request);
            }
        }
    );

    // Successfully retry same writeRecordsRequest with same records and versions, because writeRecords API is idempotent.
    await writeClient.send(params).then(
        (data) => {
            console.log("Write records successful for retry.");
        },
        (err) => {
            console.log("Error writing records:", err);
            if (err.code === 'RejectedRecordsException') {
                printRejectedRecordsException(request);
            }
        }
    );

    // upsert with lower version, this would fail because a higher version is required to update the measure value.
    version--;

    const commonAttributesWithLowerVersion = {
        'Dimensions': dimensions,
        'MeasureValueType': 'DOUBLE',
        'Time': currentTime.toString(),
        'Version': version
    };

    const updatedCpuUtilization = {
        'MeasureName': 'cpu_utilization',
        'MeasureValue': '14.5'
    };

    const updatedMemoryUtilization = {
        'MeasureName': 'memory_utilization',
        'MeasureValue': '50'
    };

    const upsertedRecords = [updatedCpuUtilization, updatedMemoryUtilization];

    const upsertedParamsWithLowerVersion = new WriteRecordsCommand({
        DatabaseName: constants.DATABASE_NAME,
        TableName: constants.TABLE_NAME,
        Records: upsertedRecords,
        CommonAttributes: commonAttributesWithLowerVersion
    });

    await writeClient.send(upsertedParamsWithLowerVersion).then(
        (data) => {
            console.log("Write records for upsert with lower version successful");
        },
        (err) => {
            console.log("Error writing records:", err);
            if (err.code === 'RejectedRecordsException') {
                printRejectedRecordsException(upsertRequestWithLowerVersion);
            }
        }
    );
    
    // upsert with higher version as new data in generated
    version = Date.now();

    const commonAttributesWithHigherVersion = {
        'Dimensions': dimensions,
        'MeasureValueType': 'DOUBLE',
        'Time': currentTime.toString(),
        'Version': version
    };

    const upsertedParamsWithHigherVerion = new WriteRecordsCommand({
        DatabaseName: constants.DATABASE_NAME,
        TableName: constants.TABLE_NAME,
        Records: upsertedRecords,
        CommonAttributes: commonAttributesWithHigherVersion
    });

    await writeClient.send(upsertedParamsWithHigherVerion).then(
        (data) => {
            console.log("Write records upsert successful with higher version");
        },
        (err) => {
            console.log("Error writing records:", err);
            if (err.code === 'RejectedRecordsException') {
                printRejectedRecordsException(upsertedParamsWithHigherVerion);
            }
        }
    );

}

export async function deleteDatabase(writeClient) {
    console.log("Deleting Database");
    const params = new DeleteDatabaseCommand({
        DatabaseName: constants.DATABASE_NAME
    })

    await writeClient.send(params).then(
        function (data) {
            console.log("Deleted database");
         },
        function(err) {
            if (err.code === 'ResourceNotFoundException') {
                console.log(`Database ${params.DatabaseName} doesn't exists.`);
            } else {
                console.log("Delete database failed.", err);
                throw err;
            }
        }
    );
}

export async function deleteTable(writeClient) {
    console.log("Deleting Table");
    const params = new DeleteTableCommand({
        DatabaseName: constants.DATABASE_NAME,
        TableName: constants.TABLE_NAME
    });

    await writeClient.send(params).then(
        function (data) {
            console.log("Deleted table");
        },
        function(err) {
            if (err.code === 'ResourceNotFoundException') {
                console.log(`Table ${params.TableName} or Database ${params.DatabaseName} doesn't exists.`);
            } else {
                console.log("Delete table failed.", err);
                throw err;
            }
        }
    );
}

function printRejectedRecordsException(request) {
    const responsePayload = JSON.parse(request.response.httpResponse.body.toString());
                console.log("RejectedRecords: ", responsePayload.RejectedRecords);
}