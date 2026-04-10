import json
import urllib.request
import re
import base64

with open('k8s/grafana-dashboard.json', 'r', encoding='utf-8') as f:
    text = f.read()

text = re.sub(r'^\s*//.*$', '', text, flags=re.MULTILINE)
text = text.replace("${DS_PROMETHEUS}", "prometheus-ds")
dash_obj = json.loads(text)

payload = {
    "dashboard": dash_obj,
    "overwrite": True
}

# Credenciais
username = "admin"
password = "admin"
auth_str = f"{username}:{password}"
encoded_auth = base64.b64encode(auth_str.encode('utf-8')).decode('utf-8')

req = urllib.request.Request(
    'http://127.0.0.1:30030/api/dashboards/db',
    data=json.dumps(payload).encode('utf-8'),
    headers={
        'Content-Type': 'application/json',
        'Authorization': f'Basic {encoded_auth}'
    },
    method='POST'
)

try:
    with urllib.request.urlopen(req) as response:
        print(response.read().decode('utf-8'))
except urllib.error.HTTPError as e:
    print(f"HTTPError: {e.code} - {e.read().decode('utf-8')}")
