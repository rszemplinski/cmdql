fetch {
    local {
        listFiles(path: "~/Downloads", showHidden: true) {
            isDirectory
            extension
            name
            size
        }
        diskSpace {
            fileSystem
            total
            used
            free
        }
        processes(limit: 5) {
          pid
          command
          elapsedTime
        }
    }
    remote(host: "ai-machine", user: "rszemplinski") {
        listFiles(path: "/var/www") {
            isDirectory
            name
            size
            date
        }
    }
    remote(host: "home-nas", user: "rszemplinski") {
        listFiles(path: "/var/www") {
            isDirectory
            name
            size
        }
        diskSpace {
            fileSystem
            total
            used
            free
        }
    }
}