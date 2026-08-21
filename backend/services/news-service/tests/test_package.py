def test_news_service_package_imports() -> None:
    import news_service

    assert news_service.__doc__ == "Praxis News Service package."

