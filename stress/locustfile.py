import uuid
import random
from locust import HttpUser, task, between

BRAZILIAN_DOCUMENT_LENGTH = 11
UNIQUE_SUFFIX_LENGTH = 6

WEIGHT_HEALTH_CHECK = 1
WEIGHT_LIST_CUSTOMERS = 5
WEIGHT_CUSTOMER_LIFECYCLE = 3

MIN_WAIT_SECONDS = 1
MAX_WAIT_SECONDS = 4

HTTP_CREATED = 201


def generate_random_document():
    digits = [str(random.randint(0, 9)) for _ in range(BRAZILIAN_DOCUMENT_LENGTH)]
    return "".join(digits)


def build_customer_payload():
    unique_suffix = uuid.uuid4().hex[:UNIQUE_SUFFIX_LENGTH]
    return {
        "name": f"Locust User {unique_suffix}",
        "email": f"locust_{unique_suffix}@stress.com",
        "document": generate_random_document()
    }


class ControlServiceUser(HttpUser):
    """Simula usuários interagindo com a ControlService API."""

    wait_time = between(MIN_WAIT_SECONDS, MAX_WAIT_SECONDS)

    @task(WEIGHT_HEALTH_CHECK)
    def health_check(self):
        self.client.get("/healthz", name="01. Health Check")

    @task(WEIGHT_LIST_CUSTOMERS)
    def list_customers(self):
        self.client.get("/api/customers", name="02. List All Customers")

    @task(WEIGHT_CUSTOMER_LIFECYCLE)
    def customer_lifecycle(self):
        customer_id = self._create_customer()
        if customer_id:
            self._get_customer(customer_id)
            self._delete_customer(customer_id)

    def _create_customer(self):
        payload = build_customer_payload()
        with self.client.post("/api/customers", json=payload, name="03. Create Customer") as response:
            if response.status_code != HTTP_CREATED:
                return None
            return response.json().get("id")

    def _get_customer(self, customer_id):
        self.client.get(f"/api/customers/{customer_id}", name="04. Get Customer Details")

    def _delete_customer(self, customer_id):
        self.client.delete(f"/api/customers/{customer_id}", name="05. Remove Customer")
