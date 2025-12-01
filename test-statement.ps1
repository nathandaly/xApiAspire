# PowerShell script for testing xAPI statements

# Test xAPI Statement - Basic Example
Write-Host "Posting basic statement..." -ForegroundColor Cyan
$response1 = Invoke-RestMethod -Uri "http://localhost:5532/xapi/statements" `
    -Method Post `
    -Headers @{
        "X-Experience-API-Version" = "2.0.0"
        "Content-Type" = "application/json"
    } `
    -Body (@{
        actor = @{
            objectType = "Agent"
            mbox = "mailto:test@example.com"
            name = "Test User"
        }
        verb = @{
            id = "http://adlnet.gov/expapi/verbs/experienced"
            display = @{
                "en-US" = "experienced"
            }
        }
        object = @{
            objectType = "Activity"
            id = "http://example.com/xapi/activities/test-course"
        }
    } | ConvertTo-Json -Depth 10)

Write-Host "Response: $($response1 | ConvertTo-Json)" -ForegroundColor Green
Write-Host "`n---`n" -ForegroundColor Gray

# Test xAPI Statement - With Result
Write-Host "Posting statement with result..." -ForegroundColor Cyan
$response2 = Invoke-RestMethod -Uri "http://localhost:5532/xapi/statements" `
    -Method Post `
    -Headers @{
        "X-Experience-API-Version" = "2.0.0"
        "Content-Type" = "application/json"
    } `
    -Body (@{
        actor = @{
            objectType = "Agent"
            mbox = "mailto:learner@example.com"
            name = "John Doe"
        }
        verb = @{
            id = "http://adlnet.gov/expapi/verbs/completed"
            display = @{
                "en-US" = "completed"
            }
        }
        object = @{
            objectType = "Activity"
            id = "http://example.com/xapi/activities/course-101"
            definition = @{
                name = @{
                    "en-US" = "Introduction to xAPI"
                }
                description = @{
                    "en-US" = "A course about the Experience API"
                }
            }
        }
        result = @{
            success = $true
            completion = $true
            score = @{
                scaled = 0.95
                raw = 95
                min = 0
                max = 100
            }
        }
        context = @{
            registration = "550e8400-e29b-41d4-a716-446655440000"
            platform = "Learning Management System"
        }
    } | ConvertTo-Json -Depth 10)

Write-Host "Response: $($response2 | ConvertTo-Json)" -ForegroundColor Green
Write-Host "`n---`n" -ForegroundColor Gray

# Retrieve all statements
Write-Host "Retrieving all statements..." -ForegroundColor Cyan
$statements = Invoke-RestMethod -Uri "http://localhost:5532/xapi/statements?limit=10" `
    -Method Get `
    -Headers @{
        "X-Experience-API-Version" = "2.0.0"
        "Accept" = "application/json"
    }

Write-Host "Found $($statements.statements.Count) statements" -ForegroundColor Green
$statements.statements | ForEach-Object {
    Write-Host "  - $($_.actor.name) $($_.verb.display.'en-US') $($_.object.id)" -ForegroundColor Yellow
}

