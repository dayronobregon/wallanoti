from dataclasses import dataclass, field
from typing import Optional

from app.domain.entity_model import EntityModel


@dataclass(frozen=True)
class ExtractionModel:
    """Resultado de extraer entidades de un texto."""
    product: EntityModel
    location: Optional[EntityModel] = None
    min_price: Optional[EntityModel] = None
    max_price: Optional[EntityModel] = None
