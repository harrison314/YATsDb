# YATsDb - Yet Another TimeSeries Database in .NET

_Yet Another TimeSeries Database_ is experimental time series database inspired _InfluxDb_ and _RavenDB_.

YATsDb is implement in pure _.NET_ and modern _C#_ with great performance, (around 30K inserts per secund on Intel i7-8550U CPU, 1.80GHz, 24GB RAM, SSD - iserting rows one at a time).

I needed a timeseries database for home IoT projects, but using _InfluxDb_ would be boring.
The implementation of the prototype, which knew how to enter data and make inquiries, took only 12 hours.

## YATsDb Features
- TimeSeries database engine (indexing using tags and time, querying)
- Javascript jobs schedule using cron expressions
- Management GUI in Blazor server
- Open API definition

_YATsDb_ is a complete database implementation with GUI, OpenAPI definitions and jobs. It requires .NET 8 runtime to run.

## YATsDb.Lite Features
- TimeSeries database engine (indexing using tags and time, querying)
- AOT compiled

_YATsDb.Lite_ is AOT compiled version with core time series database functionality.
It has no dependencies on target systems.  
The resulting binary is relatively small _< 15MB_.

## Use with cURL
The following is an example of using _YATsDb/YATsDb.Litle_ using cURL (_YATsDb_ also provides an OpenAPI endpoint definition).

In the examples, the base URL is <http://localhost:8080/>.

### Create bucket
Create a new bucket `smart_home`:
```shell
curl -XPOST http://localhost:8080/management/bucket --header "Content-Type: application/json" --data-binary "{\"bucketName\":\"smart_home\",\"description\": null}"
```

### Insert data
Enter sample data in _simple line format_ (`<measurement>[,<tag>] <value>[,<value>...n] [<timestampInMs>]`),
this format is inspired by _InfluxDb_, but does not use named values, but positional values.
Also, before _InfluxDb_, the unix timestamp in milliseconds is always used as the time.

The following sample data represent data from home measurement of sensors - temperature, humidity, light intensity.

```shell
curl -XPOST http://localhost:8080/write/smart_home --data-binary "sensors_data,Room1 22.0,45,80.4 1719242779202"
curl -XPOST http://localhost:8080/write/smart_home --data-binary "sensors_data,Room1 23.1,39.4,75.0 1719243079200"
curl -XPOST http://localhost:8080/write/smart_home --data-binary "sensors_data,Room1 23.2,41.7,NULL 1719243379246"
curl -XPOST http://localhost:8080/write/smart_home --data-binary "sensors_data,Room2 23.0,41,80.4 1719243379256"
curl -XPOST http://localhost:8080/write/smart_home --data-binary "sensors_data,Room1 22.0,45,80.4 1719243679194"
curl -XPOST http://localhost:8080/write/smart_home --data-binary "sensors_data,Room2 22.7,47.4,NULL 1719243979257"
curl -XPOST http://localhost:8080/write/smart_home --data-binary "sensors_data,Room1 22.1,47,NULL 1719244279035"
curl -XPOST http://localhost:8080/write/smart_home --data-binary "sensors_data,Room3 18.7,NULL,NULL 1719244579031"
curl -XPOST http://localhost:8080/write/smart_home --data-binary "sensors_data,Room1 24.7,51.7,2.4 1719244879109"
```

It is possible to send multiple lines in one message, but not every shell supports this.

```shell
curl -XPOST http://localhost:8080/write/smart_home --data-binary "sensors_data,Room1 24.7,51.7,2.4 1719244879109
sensors_data,Room1 24.8,55.7,3.9 1719244899109"
```

### Query data
_YATsDb_ allows querying in a syntax that resembles SQL, but uses value positions instead of names. The result of the query is a JSON result.

Select humidity data from _Room2_ older than 12 hours:
```shell
curl -XPOST http://localhost:8080/query/raw/smart_home --data-binary "SELECT AVG(1), MIN(1), MAX(1), COUNT(1) FROM sensors_data WHERE TAG('Room2') AND INTERVAL(NULL, -10h) GROUP BY +10m"
```

## Simple line protocol
Complete grammar for inserting data data - simple line protocol:

```
<lines> ::= <line> [ [\r]\n...n ]
<line> ::= <measurement> <values> [<timestamp>]
<measurement> ::= measurement_name [, tag]
<values> ::= <value> [, ...250]
<value> ::={ double_value | NULL }
<timestamp> ::= unix timestamp in milliseconds
```

### Examples

Measurement without tag:
```
sensors_data 12.5 1608852317000
```

Measurement with tag:
```
sensors_data,home/room3/esp32 12.5 1608852317000
```

Measurement with multiple values:
```
sensors_data,home/room3/esp32 12.5,-14.3,89,47.0036 1608852317000
```

Measurement with missing values (use `NULL` for missing value):
```
sensors_data,home/room4/esp32 12.5,NULL,89,47.0036 1608852317000
```

Measurement without unix timestamp - the current server time is used:
```
sensors_data,home/room3/esp32 12.5
```

Multiline values:
```
sensors_data,home/room3/esp32 12.5 1608852317000
pc_data,Workstation1 25.78,47.5,236566 1608852317900
sensors_data,home/room3/esp32 13.5 1608852417000
```

## Select statements
Complete grammar for querying data:

```
<SELECT statement> ::= SELECT <projection statement> FROM measurement_name 
    [<WHERE statement>]
    [<GROUPBY statement>]
    [<LIMIT statement>]
<projection statement> ::= { * | <projection> [ ,...250 ]}
<projection> ::= <projection function>(<index>)
<projection function> ::= { VARIANCE | SUM | COUNT | AVG | IDENTITY | MAX | MIN | STDDEV | REMEDIAN | SIGN | CHANGE_PER_SEC | CHANGE_PER_HOUR | CHANGE_PER_DAY }
<index> ::= integer_number
<WHERE statement> ::= WHERE <where function> [AND ...n]
<where function> ::= { TAG(<string>) | INTERVAL(<interval exp>, <interval exp>) }
<string> ::= string
<interval exp> ::= { integer_number | <time span> | <string> | NOW() }
<time span> ::= { + | - }double_value{ ms | s | m | h | d | w | y }
<GROUPBY statement> ::= GROUP BY <time span>
<LIMIT statement> ::= LIMIT { integer_number | integer_number, integer_number | ..integer_number }
```

### Examples

Select all data:
```
SELECT * FROM sensors_data
```

Select first 5 record from _Room3_:
```
SELECT * FROM sensors_data WHERE TAG('Room3') LIMIT 0,5
```

Select temperature data group by 10 minutes:
```
SELECT AVG(0), MIN(0), MAX(0) FROM sensors_data GROUP BY +10m
```

Select humidity data from _Room2_:
```
SELECT AVG(1), MIN(1), MAX(1), COUNT(1) FROM sensors_data WHERE TAG('Room2') GROUP BY +1d
```

Select humidity data from _Room2_ older than 12 hours:
```
SELECT AVG(1), MIN(1), MAX(1), COUNT(1) FROM sensors_data WHERE TAG('Room2') AND INTERVAL(NULL, -12h) GROUP BY +10m
```

Select last record:
```
SELECT * FROM sensors_data LIMIT ..1
```

Group all data:
```
SELECT AVG(0), AVG(1), AVG(2) FROM sensors_data
```

Select by concrete time (using unix timestamp in milliseconds):
```
SELECT AVG(0), COUNT(0) FROM sensors_data WHERE INTERVAL(1603519860000, 1608852317000) GROUP BY +10m
```

Select by concrete time (using human readable time):
```
SELECT AVG(0), COUNT(0) FROM sensors_data WHERE INTERVAL('2012-06-12 08:33:12.478', '2012-06-13') GROUP BY +10m
```

## Potential roadmap/future work
- Charts in _YATsDb_
- Authentication (eg. API key, HMAC)
- UDP endpoint
- Geo-indexing

## Known Issues
 - Problems updating a javascript job.

## Links

### Technology
* [ZoneTree](https://github.com/koculu/ZoneTree) - Thanks @koculu for the valuable advices.
* [Jint](https://github.com/sebastienros/jint)
* [Piglet](https://github.com/Dervall/Piglet) 

### Inspiration
* [InfluxDb](https://docs.influxdata.com/influxdb/v1/guides/write_data/)
* [RavenDb](https://ravendb.net/docs/article-page/6.0/csharp/document-extensions/timeseries/overview)
