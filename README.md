# Dapper.MoqTests 
- (.net framework) [![Build status](https://ci.appveyor.com/api/projects/status/rgufiqwhiliw83d6/branch/master?svg=true)](https://ci.appveyor.com/project/laingsimon/dapper-moqtests-netfx/branch/master)
- (dotnet Core) [![Build status](https://ci.appveyor.com/api/projects/status/7lus4258ta8r2g2e/branch/master?svg=true)](https://ci.appveyor.com/project/laingsimon/dapper-moqtests-core/branch/master)

Assemblies to assist testing Dapper database invocations

### Usage
```
Install-Package Dapper.MoqTests

Or

Install-Package Dapper.MoqTests.Core
```
Note: With version 1.0.0+ the library is compiled against .net 4.6.1 therefore is now compatible with .net standard projects.

Tested and confirmed to work with Dapper v1.60.0 and Moq v4.14.7

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
	where Name = @name", new { name = "anything" }, It.IsAny<IDbTransaction>(), true, null, null));
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
	where Name = @name", new { name = "to-be-deleted" }, It.IsAny<IDbTransaction>(), true, null, null));
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
		where Enabled = 1", It.IsAny<object>(), It.IsAny<IDbTransaction>(), true, null, null))
		.Return(new DataTableReader(data))
	
	repository.ReadNames();
	
	connection.Verify(c => c.Query<string>(@"select name
	from Table
	where Enabled = 1", It.IsAny<object>(), It.IsAny<IDbTransaction>(), true, null, null));
}
```

See more examples here: [Samples.cs](https://github.com/laingsimon/Dapper.MoqTests/blob/master/Dapper.MoqTests.Samples/Samples.cs)

## Features
* [x] MockDbConnection implements `IDbConnection`
* [x] Supports testing of `Query`, `Execute`, etc - see [What's supported](https://github.com/laingsimon/Dapper.MoqTests/wiki/What's-supported)
* [x] Compares SQL test case insensitively, ignoring empty lines and leading/trailing white-space
* [x] Compares parameter anonymous objects from different assemblies
* [x] Testing framework isn't restricted, can by **NUnit**, **MsTest** or anything else
* [x] Supports Strict execution, pass in the appropriate parameter to the `MockDbConnection` constructor
* [x] Supports Async data methods, from version 1.1.0
* [x] Supports verification of method execution times, from version 1.1.0
* [x] Supports verification of transaction usage, from version 1.2.0
* [x] Supports verification of stored procedures & sql text, from version 1.4.0
* [x] Supports verification of command timeout, from version 1.4.0
* [x] Supports customisation of comparisons, from version 1.5.0


## Suggestions / improvements
If you have ideas for improvements or new functionality, etc. Please contribute those ideas by way of raising an issue or creating a pull request with those changes.

I want this library to be the easiest to use by being simple and familiar. I've chosen Moq as the primary source of design at present; by no means do I think it should be tied to this.
