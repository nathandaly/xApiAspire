#!/bin/bash

# Test xAPI Statement - Basic Example
curl -X POST http://localhost:5532/xapi/statements \
  -H "X-Experience-API-Version: 2.0.0" \
  -H "Content-Type: application/json" \
  -d '{
    "actor": {
      "objectType": "Agent",
      "mbox": "mailto:test@example.com",
      "name": "Test User"
    },
    "verb": {
      "id": "http://adlnet.gov/expapi/verbs/experienced",
      "display": {
        "en-US": "experienced"
      }
    },
    "object": {
      "objectType": "Activity",
      "id": "http://example.com/xapi/activities/test-course"
    }
  }'

echo -e "\n\n---\n"

# Test xAPI Statement - With Result
curl -X POST http://localhost:5532/xapi/statements \
  -H "X-Experience-API-Version: 2.0.0" \
  -H "Content-Type: application/json" \
  -d '{
    "actor": {
      "objectType": "Agent",
      "mbox": "mailto:learner@example.com",
      "name": "John Doe"
    },
    "verb": {
      "id": "http://adlnet.gov/expapi/verbs/completed",
      "display": {
        "en-US": "completed"
      }
    },
    "object": {
      "objectType": "Activity",
      "id": "http://example.com/xapi/activities/course-101",
      "definition": {
        "name": {
          "en-US": "Introduction to xAPI"
        },
        "description": {
          "en-US": "A course about the Experience API"
        }
      }
    },
    "result": {
      "success": true,
      "completion": true,
      "score": {
        "scaled": 0.95,
        "raw": 95,
        "min": 0,
        "max": 100
      }
    },
    "context": {
      "registration": "550e8400-e29b-41d4-a716-446655440000",
      "platform": "Learning Management System"
    }
  }'

echo -e "\n\n---\n"

# Test xAPI Statement - Failed Attempt
curl -X POST http://localhost:5532/xapi/statements \
  -H "X-Experience-API-Version: 2.0.0" \
  -H "Content-Type: application/json" \
  -d '{
    "actor": {
      "objectType": "Agent",
      "mbox": "mailto:learner@example.com",
      "name": "Jane Smith"
    },
    "verb": {
      "id": "http://adlnet.gov/expapi/verbs/attempted",
      "display": {
        "en-US": "attempted"
      }
    },
    "object": {
      "objectType": "Activity",
      "id": "http://example.com/xapi/activities/quiz-1"
    },
    "result": {
      "success": false,
      "completion": true,
      "score": {
        "scaled": 0.45,
        "raw": 45,
        "min": 0,
        "max": 100
      },
      "response": "Selected incorrect answers"
    }
  }'

echo -e "\n\n---\n"

# Retrieve all statements
echo "Retrieving all statements:"
curl -X GET "http://localhost:5532/xapi/statements?limit=10" \
  -H "X-Experience-API-Version: 2.0.0" \
  -H "Accept: application/json"

echo -e "\n"

