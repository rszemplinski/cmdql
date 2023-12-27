fetch {
    local {
        listFiles(path: "~/Downloads") {
            name
            size
            date
            permissions
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
    remote(host: "adguard", user: "ubuntu") {
        listFiles(path: "/var/www") {
            name
            size
            owner
        }
    }
}