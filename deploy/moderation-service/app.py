"""
Moderation Service - Simulates a rate-limited external moderation API.

Features:
- 400ms average latency with random wobble (350-450ms)
- Max 5 concurrent requests (returns 429 if exceeded)
- Returns X-RateLimit-Concurrent-Max header on rate limit errors
"""

import base64
import json
import random
import threading
import time
from flask import Flask, request, jsonify, Response

app = Flask(__name__)

# Configuration
MAX_CONCURRENT = 5
BASE_DELAY_MS = 400
WOBBLE_MS = 50  # +/- 50ms

# Thread-safe counter for concurrent requests
concurrent_lock = threading.Lock()
concurrent_count = 0


@app.route("/health", methods=["GET"])
def health():
    return jsonify({"status": "healthy"})


@app.route("/moderate", methods=["POST"])
def moderate():
    global concurrent_count

    # Check and increment concurrent count
    with concurrent_lock:
        if concurrent_count >= MAX_CONCURRENT:
            return Response(
                json.dumps({
                    "error": "Too many concurrent requests",
                    "message": f"Maximum {MAX_CONCURRENT} concurrent requests allowed"
                }),
                status=429,
                mimetype="application/json",
                headers={"X-RateLimit-Concurrent-Max": str(MAX_CONCURRENT)}
            )
        concurrent_count += 1

    try:
        # Simulate processing delay with wobble
        delay_ms = BASE_DELAY_MS + random.randint(-WOBBLE_MS, WOBBLE_MS)
        time.sleep(delay_ms / 1000.0)

        # Generate a passing score (for this exercise, all images pass)
        score = random.randint(80, 95)
        passed = score >= 70

        # Create signature (base64 encoded JSON with score)
        payload = {"moderationScore": score}
        signature = base64.b64encode(json.dumps(payload).encode()).decode()

        return jsonify({
            "signature": signature,
            "score": score,
            "passed": passed
        })

    finally:
        # Decrement concurrent count
        with concurrent_lock:
            concurrent_count -= 1


if __name__ == "__main__":
    app.run(host="0.0.0.0", port=5003, threaded=True)
