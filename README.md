<p align="center">
  <img src='./assets/qlsh.png' height="200">
</p>

## 📖 Overview

`qlsh` is a revolutionary CLI tool designed for developers, DevOps engineers, and system administrators. This tool blends the power of query language with the flexibility and familiarity of a shell environment, providing an intuitive, efficient, and powerful way to manage and query data across various systems and services.

`qlsh` is built to streamline the process of monitoring, managing, and analyzing data from servers, databases, applications, and other sources, all from the comfort of your command line.

## ⚡️ Features

* **GraphQL-like Querying**: Leverage a syntax reminiscent of GraphQL to fetch, mutate, and subscribe to data points across your infrastructure and services. This intuitive querying mechanism makes data retrieval and manipulation as simple as writing a query.

* **Multi-Source Data Aggregation:** Aggregate data from multiple sources such as server metrics, application logs, network statistics, and more, all within a single query framework.

* **Real-Time Monitoring and Alerts**: Monitor your systems in real-time and set up alerts based on custom thresholds and conditions, ensuring you're always informed about the health and performance of your infrastructure.

* **Integrated Scripting Capabilities**: Embed and execute shell scripts or external programs directly within qlsh, providing a seamless experience for complex operations.

* **Advanced Data Manipulation**: Utilize advanced data operations like map, reduce, and filter within queries, enabling sophisticated data analysis and transformation right in the CLI.

* **Customizable and Extensible**: Extend `qlsh` with custom plugins, scripts, and integrations, tailoring the tool to fit your specific needs and workflows.

## Examples

### Input:
```
fetch {
  local {
    fileSpace(path: "/var/www") {
      size
      path
    }
    listFiles(path: "/var/www") {
      name
      size
      permissions
    }
    currentTime {
      year
      timezone
    }
  }
}
```

### Output:
```json
{
  "localhost": {
    "fileSpace": {
        "size": 69420,
        "path": "/var/www"
    },
    "listFiles": [
      {
        "name": "index.html",
        "size": 420,
        "permissions": "rw-r--r--"
      },
      {
        "name": "app.js",
        "size": 69000,
        "permissions": "rw-r--r--"
      }
    ],
    "currentTime": {
      "year": 2024,
      "timezone": "MST"
    }
  }
}
```

## 🚨 Disclaimer! 🚨

This is very much a work in progress. Nothing is properly tested, documentation is very limited, and the API is subject to change at any time. Use at your own risk.