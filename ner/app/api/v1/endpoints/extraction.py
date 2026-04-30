from fastapi import APIRouter, Depends, HTTPException, Query, status
from pydantic import BaseModel

from app.api.dependencies import get_extract_use_case
from app.application.entity_extractor.extract_entities import ExtractEntities
from app.domain.extraction_model import ExtractionModel

router = APIRouter()


# ── Schemas (Response) ───────────────────────────────────────────────────────

class ExtractionResponse(BaseModel):
    text: str
    product: str | None
    location: str | None
    min_price: str | None
    max_price: str | None


# ── Endpoint ──────────────────────────────────────────────────────────────────

@router.get(
    "/extract",
    response_model=ExtractionResponse,
    summary="Extrae entidades de un texto",
    description=(
        "Recibe un texto libre y devuelve las entidades reconocidas: "
        "producto, ubicación, precio mínimo y precio máximo."
    ),
)
def extract_entities(
    text: str = Query(..., min_length=1, description="Texto a analizar"),
    use_case: ExtractEntities = Depends(get_extract_use_case),
) -> ExtractionResponse:
    try:
        result: ExtractionModel = use_case.execute(text)
    except ValueError as exc:
        raise HTTPException(
            status_code=status.HTTP_422_UNPROCESSABLE_ENTITY,
            detail=str(exc),
        ) from exc

    return ExtractionResponse(
        text=text,
        product=result.product.value if result.product else None,
        location=result.location.value if result.location else None,
        min_price=result.min_price.value if result.min_price else None,
        max_price=result.max_price.value if result.max_price else None,
    )
