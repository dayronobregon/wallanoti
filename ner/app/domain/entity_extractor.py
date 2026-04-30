from abc import ABC, abstractmethod

from app.domain.extraction_model import ExtractionModel


class NerExtractor(ABC):
    """Define qué debe saber hacer cualquier extractor de entidades NER."""

    @abstractmethod
    def extract(self, text: str) -> ExtractionModel:
        ...
