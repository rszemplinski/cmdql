<p align="center">
  <img src='https://github.com/rszemplinski/qlsh/blob/main/assets/QLSH.png' height="200">
</p>

## 📖 Overview

`qlsh` is a CLI tool designed for developers, DevOps engineers, and system administrators. This tool blends the power of query language with the flexibility and familiarity of a shell environment, providing an intuitive, efficient, and powerful way to manage and query data across various systems and services.

`qlsh` is built to streamline the process of monitoring, managing, and analyzing data from servers, databases, applications, and other sources, all from the comfort of your command line.

## ⚡️ Features

* **GQL-like Querying**: Leverage a syntax reminiscent of GraphQL to fetch, mutate, and subscribe to data points across your infrastructure and services. This intuitive querying mechanism makes data retrieval and manipulation as simple as writing a query.

* **Multi-Source Data Aggregation:** Aggregate data from multiple sources such as server metrics, application logs, network statistics, and more, all within a single query framework.

* **Real-Time Monitoring and Alerts**: Monitor your systems in real-time and set up alerts based on custom thresholds and conditions, ensuring you're always informed about the health and performance of your infrastructure.

* **Integrated Scripting Capabilities**: Embed and execute shell scripts or external programs directly within qlsh, providing a seamless experience for complex operations.

* **Advanced Data Manipulation**: Utilize advanced data operations like map, reduce, and filter within queries, enabling sophisticated data analysis and transformation right in the CLI.

* **Customizable and Extensible**: Extend `qlsh` with custom plugins, scripts, and integrations, tailoring the tool to fit your specific needs and workflows.

## Installation

Currently, the only way to use `qlsh` is to clone the repository and run the tool from source.

```bash
git clone git@github.com:rszemplinski/qlsh.git
```

## Examples

To try one of the examples, run `dotnet run --project src/QL.Engine -i examples/example.qlsh`.

### Input:
```
fetch {
    local {
        listFiles(path: "~/Pictures") {
            name
            size | toMegabytes
        }
        currentTime {
            timezone
            year
        }
        diskSpace {
            used | toGigabytes
            free | toGigabytes
            total
            fileSystem
        }
    }
}
```

### Output:
```json
{
  "localhost": {
    "listFiles": [
      {
        "size": 0.00390625,
        "name": "screenshots"
      },
      {
        "size": 0.00390625,
        "name": "Screenshots"
      }
    ],
    "currentTime": {
      "timezone": "MST",
      "year": 2024
    },
    "diskSpace": [
      {
        "fileSystem": "tmpfs",
        "total": 3355750400,
        "used": 0.002349853515625,
        "free": 3.122936248779297
      },
      {
        "fileSystem": "efivarfs",
        "total": 126976,
        "used": 5.14984130859375E-05,
        "free": 6.67572021484375E-05
      },
      {
        "fileSystem": "/dev/nvme1n1p3",
        "total": 924293296128,
        "used": 389.6685791015625,
        "free": 471.14663314819336
      }
    ]
  }
}

```

## 🚨 Disclaimer! 🚨

This is very much a work in progress. Nothing is properly tested, documentation is very limited, and the API is subject to change at any time. Use at your own risk.
