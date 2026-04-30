from fastapi import APIRouter

from app.api.v1.endpoints.extraction import router as extraction_router

router = APIRouter(prefix="/v1")
router.include_router(extraction_router, prefix="/ner", tags=["NER"])
