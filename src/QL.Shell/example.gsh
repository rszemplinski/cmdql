fetch {
    local {
        listFiles(path: "/Users/ryan.szemplinski/Downloads") {
            name
            size
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
        }
    }
    remote(host: "adguard", user: "ubuntu") {
        listFiles(path: "/var/www") {
            name
            size
        }
    }
}