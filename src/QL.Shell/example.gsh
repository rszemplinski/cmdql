fetch {
    local {
        docker {
            containers {
                names
                command
            }
        }
        listFiles(path: "~") {
            name
            size | toGigabytes(format: "#.##")
        }
    }
    remote(host: "ai-machine", user: "rszemplinski") {
        currentTime {
            timezone
            year
        }
        docker {
            containers {
                names
                command
            }
        }
    }
}