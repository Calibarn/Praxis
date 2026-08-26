from news_service.app import create_app


def _schema() -> dict:
    app = create_app(database_url="mysql+asyncmy://unused:unused@unused/unused")
    return app.openapi()


def test_news_endpoint_exposes_page_and_page_size_query_parameters() -> None:
    schema = _schema()
    parameters = {
        parameter["name"]: parameter
        for parameter in schema["paths"]["/api/news"]["get"]["parameters"]
    }

    assert parameters["page"]["schema"]["minimum"] == 1
    assert parameters["pageSize"]["schema"]["minimum"] == 1
    assert parameters["pageSize"]["schema"]["maximum"] == 100


def test_news_endpoint_documents_the_contract_response_codes() -> None:
    schema = _schema()
    responses = schema["paths"]["/api/news"]["get"]["responses"]

    assert {"200", "422"} <= set(responses)


def test_health_endpoint_is_documented() -> None:
    schema = _schema()

    assert "/health" in schema["paths"]


def test_news_item_schema_matches_the_public_contract_fields() -> None:
    schema = _schema()
    properties = set(schema["components"]["schemas"]["NewsItem"]["properties"])

    assert properties == {
        "id",
        "title",
        "summary",
        "content",
        "publishedAt",
        "validFrom",
        "validUntil",
    }


def test_news_page_schema_matches_the_public_contract_fields() -> None:
    schema = _schema()
    properties = set(schema["components"]["schemas"]["NewsPage"]["properties"])

    assert properties == {"items", "page", "pageSize", "total", "hasMore"}


def test_news_page_schema_documents_the_contract_numeric_bounds() -> None:
    schema = _schema()
    properties = schema["components"]["schemas"]["NewsPage"]["properties"]

    assert properties["page"]["minimum"] == 1
    assert properties["pageSize"]["minimum"] == 1
    assert properties["pageSize"]["maximum"] == 100
    assert properties["total"]["minimum"] == 0


def test_problem_schema_matches_the_public_contract_fields() -> None:
    schema = _schema()
    properties = set(schema["components"]["schemas"]["Problem"]["properties"])

    assert properties == {"code", "message"}
