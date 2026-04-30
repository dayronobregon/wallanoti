from __future__ import annotations

import random
import re
from dataclasses import dataclass
from typing import List, Optional, Tuple

# ---------------------------------------------------------------------------
# Vocabulario
# ---------------------------------------------------------------------------

PRODUCTS = [
    # Electrónica
    "iphone",
    "iphone 13",
    "iphone 14",
    "iphone 15",
    "samsung galaxy",
    "samsung galaxy s23",
    "xiaomi redmi",
    "xiaomi redmi note",
    "macbook air",
    "macbook pro",
    "portátil lenovo",
    "portátil hp",
    "tablet samsung",
    "tablet ipad",
    "ps5",
    "xbox series x",
    "nintendo switch",
    "piezas de nintendo",
    "televisor",
    "televisor lg",
    "televisor samsung",
    "televisor 50 pulgadas",   # sin "de" — evita colisión con "{product} de {precio}"
    "auriculares sony",
    "airpods",
    "smartwatch",
    "cámara canon",
    # Hogar
    "sofá",
    "mesa comedor",            # sin "de" — idem
    "silla oficina",           # sin "de" — idem
    "nevera",
    "lavadora",
    "microondas",
    "aspiradora",
    "cama",
    "armario",
    "escritorio",
    # Moda y complementos
    "anillos",
    "pulseras",
    "collar oro",              # sin "de" — idem
    "zapatos nike",
    "zapatillas adidas",
    "bolso cuero",             # sin "de" — idem
    "chaqueta cuero",          # sin "de" — idem
    "gafas sol",               # sin "de" — idem
    # Deporte y ocio
    "bicicleta eléctrica",
    "bicicleta montaña",       # sin "de" — idem
    "patinete eléctrico",
    "raqueta tenis",           # sin "de" — idem
    "saco boxeo",              # sin "de" — idem
    # Vehículos y piezas
    "coche eléctrico",
    "moto honda",
    "patines línea",           # sin "de" — idem
    # Otros
    "libros cocina",           # sin "de" — idem
    "guitarra eléctrica",
    "piano digital",
    "cuna bebé",               # sin "de" — idem
    "piezas nintendo",         # sin "de" — idem
]

LOCATIONS = [
    "murcia",
    "madrid",
    "barcelona",
    "valencia",
    "sevilla",
    "zaragoza",
    "alicante",
    "málaga",
    "granada",
    "bilbao",
    "valladolid",
    "córdoba",
    "vigo",
    "gijón",
    "palma de mallorca",
    "las palmas",
    "santa cruz de tenerife",
    "pamplona",
    "san sebastián",
    "burgos",
]

# ---------------------------------------------------------------------------
# Plantillas por tipo de búsqueda
# ---------------------------------------------------------------------------

# Precio máximo explícito (palabras como "máximo", "hasta", "menos de")
MAX_TEMPLATES = [
    "quiero un {product} por menos de {max_price} euros en {location}",
    "busco {product} en {location} por debajo de {max_price} euros",
    "necesito {product} en {location} hasta {max_price}€",
    "en {location} quiero comprar {product} máximo {max_price} euros",
    "{product} máximo {max_price} euros en {location}",
    "{product} máximo {max_price} euros",
    "{product} máximo {max_price}",
    "en {location} busco {product} por menos de {max_price}€",
    "{product} hasta {max_price} euros en {location}",
    "{product} hasta {max_price}€",
    "busco {product} que no pase de {max_price} euros en {location}",
    "necesito {product} por debajo de {max_price} euros",
    "{product} en {location} no más de {max_price} euros",
    "{product} no más de {max_price} euros en {location}",
    "{product} no más de {max_price}€ en {location}",
    "{product} no más de {max_price} euros",
    "{product} no más de {max_price}€",
    "quiero {product} en {location} no más de {max_price} euros",
    "busco {product} en {location} no más de {max_price}€",
    "necesito {product} no más de {max_price} euros en {location}",
    "{product} en {location} de {max_price} euros",
    "{product} en {location} de {max_price}",
    "{product} de {max_price}",
]

# Precio mínimo explícito
MIN_TEMPLATES = [
    "quiero un {product} en {location} por más de {min_price} euros",
    "busco {product} en {location} desde {min_price}€",
    "en {location} necesito {product} mínimo {min_price} euros",
    "{product} en {location} a partir de {min_price} euros",
    "{product} en {location} a partir de {min_price}",
    "{product} desde {min_price} euros en {location}",
    "{product} mínimo {min_price}€",
    "{product} mínimo {min_price} euros",
    "busco {product} de más de {min_price} euros en {location}",
    "necesito {product} por encima de {min_price} euros en {location}",
]

# Rango de precio
RANGE_TEMPLATES = [
    "quiero un {product} entre {min_price} y {max_price} euros en {location}",
    "busco {product} en {location} desde {min_price} hasta {max_price} euros",
    "necesito {product} en {location} de {min_price} a {max_price} euros",
    "en {location} quiero un {product} entre {min_price}€ y {max_price}€",
    "{product} entre {min_price}€ y {max_price}€ en {location}",
    "{product} entre {min_price}€ y {max_price}",
    "{product} entre {min_price} y {max_price} euros",
    "busco {product} de {min_price} a {max_price} euros en {location}",
    "en {location} {product} entre {min_price} y {max_price}€",
]

# Precio implícito como tope — "de X euros", "a X euros", "por X euros"
# El usuario no dice "máximo" pero el precio actúa como MAX_PRICE
IMPLICIT_MAX_TEMPLATES = [
    "quiero un {product} de {max_price} euros en {location}",
    "quiero un {product} de {max_price}€ en {location}",
    "quiero un {product} de {max_price} euros",
    "quiero un {product} de {max_price}€",
    "busco {product} a {max_price} euros en {location}",
    "busco {product} a {max_price}€ en {location}",
    "busco {product} a {max_price} euros",
    "{product} a {max_price} euros en {location}",
    "{product} a {max_price}€ en {location}",
    "{product} a {max_price}€",
    "{product} por {max_price} euros en {location}",
    "{product} por {max_price}€ en {location}",
    "{product} por {max_price} euros",
    "necesito {product} por {max_price}€ en {location}",
    "en {location} quiero {product} a {max_price} euros",
    "en {location} busco {product} por {max_price}€",
]

# Sin precio — solo producto y/o ubicación
NO_PRICE_WITH_LOCATION_TEMPLATES = [
    "busco {product} en {location}",
    "quiero un {product} en {location}",
    "necesito {product} en {location}",
    "en {location} busco {product}",
    "en {location} quiero {product}",
    "en {location} necesito {product}",
    "{product} en {location}",
    "alguien vende {product} en {location}",
    "{product} en {location}",
    "hay {product} en {location}",
    "quiero comprar {product} en {location}",
    "me interesa {product} en {location}",
]

NO_PRICE_WITHOUT_LOCATION_TEMPLATES = [
    "busco {product}",
    "quiero un {product}",
    "necesito {product}",
    "quiero comprar {product}",
    "me interesa un {product}",
    "compro {product}",
    "busco {product} de segunda mano",
    "quiero {product} nuevo",
]


# ---------------------------------------------------------------------------
# Núcleo del generador
# ---------------------------------------------------------------------------

@dataclass
class NERExample:
    text: str
    entities: List[Tuple[int, int, str]]


def _find_span(text: str, value: str, cursor: int = 0) -> Tuple[int, int]:
    """Localiza `value` en `text` desde `cursor`, respetando límites de palabra.

    Para valores numéricos (precios) usa \\b para evitar que "500" matchee
    dentro de "2500" o "50 pulgadas", lo que generaría anotaciones corruptas.
    Para valores de texto (producto, ubicación) usa búsqueda literal insensible
    a mayúsculas — los nombres compuestos no necesitan límite de palabra estricto.
    """
    if value.isdigit():
        pattern = re.compile(r"\b" + re.escape(value) + r"\b")
        match = pattern.search(text.lower(), cursor)
        if match is None:
            raise ValueError(f"No se encontró '{value}' (con límite de palabra) en: '{text}'")
        return match.start(), match.end()

    start = text.lower().find(value.lower(), cursor)
    if start == -1:
        raise ValueError(f"No se encontró '{value}' en: '{text}'")
    return start, start + len(value)


def _build_example(
        template: str,
        product: str,
        location: Optional[str],
        min_price: Optional[int],
        max_price: Optional[int],
) -> NERExample:
    values = {
        "product": product,
        "location": location or "",
        "min_price": str(min_price) if min_price is not None else "",
        "max_price": str(max_price) if max_price is not None else "",
    }

    text = template.format(**values)
    entities: List[Tuple[int, int, str]] = []

    product_start, product_end = _find_span(text, product)
    entities.append((product_start, product_end, "PRODUCT"))

    if "{location}" in template and location:
        loc_start, loc_end = _find_span(text, location)
        entities.append((loc_start, loc_end, "LOCATION"))

    if min_price is not None:
        min_str = str(min_price)
        # Los precios siempre aparecen después del producto en los templates,
        # así que buscamos desde product_end para evitar matchear números
        # que formen parte del nombre del producto (ej. "televisor 50 pulgadas").
        min_start, min_end = _find_span(text, min_str, cursor=product_end)
        entities.append((min_start, min_end, "MIN_PRICE"))

    if max_price is not None:
        max_str = str(max_price)
        # Ídem — buscar desde product_end.
        # Si además hay min_price, buscar después de él para evitar colisión
        # cuando min_price == max_price (raro pero posible).
        price_cursor = product_end
        if min_price is not None:
            price_cursor = entities[-1][1]  # fin del span MIN_PRICE ya anotado
        max_start, max_end = _find_span(text, max_str, cursor=price_cursor)
        entities.append((max_start, max_end, "MAX_PRICE"))

    return NERExample(text=text, entities=entities)


# ---------------------------------------------------------------------------
# Pesos de muestreo — refleja distribución realista de búsquedas
# ---------------------------------------------------------------------------
#
#  implicit_max  → patrón más común en lenguaje natural ("un iphone de 150")
#  no_price      → muy frecuente, usuarios que solo buscan por producto/zona
#  max           → explícito pero habitual
#  range         → menos común
#  min           → el menos común
#
_SAMPLE_TYPES = ["max", "min", "range", "implicit_max", "no_price"]
_WEIGHTS = [0.20, 0.10, 0.15, 0.30, 0.25]


def build_dataset(size: int = 2500, seed: int = 42) -> List[Tuple[str, dict]]:
    random.seed(seed)

    examples: List[Tuple[str, dict]] = []

    for _ in range(size):
        product = random.choice(PRODUCTS)
        location = random.choice(LOCATIONS)

        sample_type = random.choices(_SAMPLE_TYPES, weights=_WEIGHTS, k=1)[0]

        if sample_type == "max":
            max_price = random.randrange(80, 2200, 10)
            template = random.choice(MAX_TEMPLATES)
            ex = _build_example(template, product, location, min_price=None, max_price=max_price)

        elif sample_type == "min":
            min_price = random.randrange(50, 1800, 10)
            template = random.choice(MIN_TEMPLATES)
            ex = _build_example(template, product, location, min_price=min_price, max_price=None)

        elif sample_type == "range":
            min_price = random.randrange(50, 1200, 10)
            max_price = random.randrange(min_price + 20, 2400, 10)
            template = random.choice(RANGE_TEMPLATES)
            ex = _build_example(template, product, location, min_price=min_price, max_price=max_price)

        elif sample_type == "implicit_max":
            max_price = random.randrange(30, 2200, 10)
            template = random.choice(IMPLICIT_MAX_TEMPLATES)
            ex = _build_example(template, product, location, min_price=None, max_price=max_price)

        else:  # no_price
            if random.random() < 0.6:
                # 60 % con location
                template = random.choice(NO_PRICE_WITH_LOCATION_TEMPLATES)
                ex = _build_example(template, product, location, min_price=None, max_price=None)
            else:
                # 40 % solo producto, sin location
                template = random.choice(NO_PRICE_WITHOUT_LOCATION_TEMPLATES)
                ex = _build_example(template, product, None, min_price=None, max_price=None)

        examples.append((ex.text, {"entities": ex.entities}))

    return examples
