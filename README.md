# Dapper.MoqTests
Assemblies to assist testing Dapper database invocations

### Usage
```
Install-Package Dapper.MoqTests
```
Note: With version 1.0.0+ the library is compiled against .net 4.6.1 therefore is now compatible with .net standard projects.

Tested and confirmed to work with Dapper v1.60.0 and Moq v4.5.3

### For example if you want to test this call:
```csharp
connection.Query<int>(@"select count(*)
from Table
where Name = @name", new { name = "anything" })
```
### You can do this in a test:
```csharp
using Dapper.MoqTests;

public void Test()
{
	var connection = new MockDbConnection();
	var repository = new MyTestRepository(connection);
	
	repository.ReadNames();
	
	connection.Verify(c => c.Query<int>(@"select count(*)
	from Table
	where Name = @name", new { name = "anything" }));
}
```
### It doesn't have to return anything:
```csharp
using Dapper.MoqTests;

public void Test()
{
	var connection = new MockDbConnection();
	var repository = new MyTestRepository(connection);
	
	repository.DeleteSomething();
	
	connection.Verify(c => c.Execute(@"delete 
	from Table 
	where Name = @name", new { name = "to-be-deleted" }));
}
```
### You can also setup return values, like this:
```csharp
using Dapper.MoqTests;
using System.Data;

public void Test()
{
	var connection = new MockDbConnection();
	var repository = new MyTestRepository(connection);
	var data = new DataTable
	{
		Columns = 
		{
			"name"
		},
		Rows = 
		{
			"Bloggs",
			"Smith"
		}
	};
	connection
		.Setup(c => c.Query<string>(@"select name 
		from Table
		where Enabled = 1"))
		.Return(new DataTableReader(data))
	
	repository.ReadNames();
	
	connection.Verify(c => c.Query<string>(@"select name
	from Table
	where Enabled = 1", It.IsAny<object>()));
}
```

## Features
* [x] MockDbConnection implements `IDbConnection`
* [x] Supports testing of `Query` and `QuerySingle`
* [x] Supports testing of `Execute`
* [x] Compares SQL test case insensitively, ignoring empty lines and leading/trailing white-space
* [x] Compares parameter anonymous objects from different assemblies
* [x] Testing framework isn't restricted, can by **NUnit**, **MsTest** or anything else
* [x] Supports Strict execution, pass in the appropriate parameter to the `MockDbConnection` constructor
* [x] Supports Async data methods, from version 1.1.0
* [x] Supports verification of method execution times, from version 1.1.0

## Limitations
* [ ] If you have not set-up a `Query<T>()` or `QuerySingle<T>()`, you can only verify by `Query<object>()` or `QuerySingle<object>()`

## Suggestions / improvements
Raise an issue and I'll see what I can do

Happy testing, let me know if you see any issues and I'll do what I can to resolve them.