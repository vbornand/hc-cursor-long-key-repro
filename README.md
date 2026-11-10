# Cursor paging fails on long string sort keys

Minimal reproduction for GreenDonut.Data 16.7.4 (also 16.6.2 and `main`).

Keyset paging (`ToPageAsync`) sorted on a string column throws while building the
end cursor as soon as the last row's sort key is longer than about 254 UTF-8 bytes.

## Run

```shell
dotnet run
```

Requires the .NET 10 SDK. SQLite runs in memory, nothing to set up.

## Output

```text
OK    200 chars, 200 UTF-8 bytes: end cursor has 272 chars
FAIL  300 chars, 300 UTF-8 bytes:
System.InvalidOperationException: Operation is not valid due to the current state of the object.
   at GreenDonut.Data.Cursors.CursorFormatter.Format[T](T entity, CursorKey[] keys, CursorPageInfo pageInfo)
   ...
FAIL  150 chars, 300 UTF-8 bytes:
System.InvalidOperationException: Operation is not valid due to the current state of the object.
   ...
```
