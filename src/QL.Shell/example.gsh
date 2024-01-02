fetch {
    local {
        fileSpace(path: "~/projects", depth: 0) {
            size
            path
        }
        processes(limit: 5) {
            pid
            flags
        }
        getLogs(startDate: "2023-12-25", endDate: "2023-12-31", top: 50) {
            message
            timestamp
        }
    }
    remote(host: "ai-machine", user: "rszemplinski") {
        listFiles(path: "/var/www") {
            name
            size | toGigabytes
        }
        currentTime {
            timezone
            year
        }
    }
}