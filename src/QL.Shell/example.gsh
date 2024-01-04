fetch {
    local {
        listFiles(path: "~/Pictures") {
            name
            size | toMegabytes
        }
    }
    remote(host: "ai-machine", user: "rszemplinski") {
        currentTime {
            timezone
            year
        }
    }
}