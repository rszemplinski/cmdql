fetch {
    local {
        docker {
            containers {
                names
                command
            }
        }
        currentTime {
            timezone
            year
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