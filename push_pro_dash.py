# push_pro_dash.py
# Importa o dashboard do ControlService no Grafana.
# Pré-requisito: kubectl port-forward service/grafana-service 3000:3000
#
# Uso (a partir da raiz do projeto):
#   kubectl port-forward service/grafana-service 3000:3000
#   python push_pro_dash.py

import json
import urllib.request
import urllib.error
import re
import base64
import os
import sys

# Localiza o JSON relativo à raiz do projeto (onde este script está)
SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
DASHBOARD_PATH = os.path.join(SCRIPT_DIR, 'infra-shared', 'k8s', 'grafana-dashboard.json')

# URL do Grafana via port-forward (padrão: localhost:3000)
GRAFANA_URL = os.environ.get('GRAFANA_URL', 'http://localhost:3000')

print(f'==> Lendo dashboard: {DASHBOARD_PATH}')
if not os.path.exists(DASHBOARD_PATH):
    print(f'ERRO: arquivo não encontrado: {DASHBOARD_PATH}')
    sys.exit(1)

with open(DASHBOARD_PATH, 'r', encoding='utf-8') as f:
    text = f.read()

text = re.sub(r'^\s*//.*$', '', text, flags=re.MULTILINE)
text = text.replace('${DS_PROMETHEUS}', 'prometheus-ds')
dash_obj = json.loads(text)

payload = {
    'dashboard': dash_obj,
    'overwrite': True
}

username = 'admin'
password = 'admin'
encoded_auth = base64.b64encode(f'{username}:{password}'.encode()).decode()

print(f'==> Enviando dashboard para {GRAFANA_URL} ...')
req = urllib.request.Request(
    f'{GRAFANA_URL}/api/dashboards/db',
    data=json.dumps(payload).encode('utf-8'),
    headers={
        'Content-Type': 'application/json',
        'Authorization': f'Basic {encoded_auth}'
    },
    method='POST'
)

try:
    with urllib.request.urlopen(req) as response:
        result = json.loads(response.read().decode())
        print(f'==> Dashboard importado com sucesso!')
        print(f'    Titulo : {dash_obj.get("title", "?")}' )
        print(f'    UID    : {result.get("uid", "?")}' )
        print(f'    URL    : {GRAFANA_URL}/d/{result.get("uid", "")}' )
except urllib.error.HTTPError as e:
    body = e.read().decode('utf-8')
    print(f'ERRO HTTP {e.code}: {body}')
    print('Verifique se o port-forward está ativo: kubectl port-forward service/grafana-service 3000:3000')
    sys.exit(1)
except urllib.error.URLError as e:
    print(f'ERRO de conexão: {e.reason}')
    print('Verifique se o port-forward está ativo: kubectl port-forward service/grafana-service 3000:3000')
    sys.exit(1)
