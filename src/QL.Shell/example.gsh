fetch {
    local {
        fileSpace(path: "~/go", depth: 0) {
            size
            path
        }
        processes(limit: 5) {
            pid
            flags
        }
    }
    remote(host: "ai-machine", user: "rszemplinski") {
        listFiles(path: "/var/www") {
            name
            size | toGigabytes
        }
    }
}