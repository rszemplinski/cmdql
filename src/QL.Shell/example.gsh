fetch {
    local {
        listFiles(path: "~/Downloads") {
            name
            size
            date
            permissions
        }
        diskSpace {
            total
            used
            free
        }
    }
    remote(host: "ai-machine", user: "rszemplinski") {
        listFiles(path: "/var/www") {
            name
            size
        }
    }
    remote(host: "home-nas", user: "rszemplinski") {
        listFiles(path: "/var/www") {
            name
            size
            permissions
        }
    }
}