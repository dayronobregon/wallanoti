from app.domain.extraction_model import ExtractionModel
from app.domain.entity_extractor import NerExtractor


class ExtractEntities:
    """
    Caso de uso: extraer entidades de un texto libre.

    Orquesta el dominio y delega la inferencia al NERExtractor.
    No sabe nada de spaCy, FastAPI ni HTTP.
    """

    def __init__(self, ner_model: NerExtractor) -> None:
        self._ner_model = ner_model

    def execute(self, text: str) -> ExtractionModel:
        if not text or not text.strip():
            raise ValueError("El texto no puede estar vacío.")

        return self._ner_model.extract(text.strip())
