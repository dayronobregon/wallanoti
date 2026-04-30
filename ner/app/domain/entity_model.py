from dataclasses import dataclass
from enum import Enum


class EntityType(Enum):
    Product = "Product"
    Location = "Location"
    MinPrice = "MinPrice"
    MaxPrice = "MaxPrice"


@dataclass(frozen=True)
class EntityModel:
    classification: EntityType
    value: str
