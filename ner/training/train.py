import spacy
import random
import sys
from pathlib import Path
from spacy.training import Example
from spacy.util import minibatch, compounding
from data.generate_data import build_dataset

# Agregar el directorio raíz al path
sys.path.insert(0, str(Path(__file__).parent.parent))


# ---------------------------
# 1. CREAR MODELO SOLO CON NER
# ---------------------------
# Crear un modelo en blanco solo con NER (sin otros pipes)
nlp = spacy.blank("es")

# Agregar solo el pipe NER
if "ner" not in nlp.pipe_names:
    ner = nlp.add_pipe("ner")
else:
    ner = nlp.get_pipe("ner")

# ---------------------------
# 2. PREPARAR DATOS
# ---------------------------
dataset = build_dataset(seed=42)

# Extraer labels únicos del dataset
labels = {label for _, ann in dataset for _, _, label in ann["entities"]}

print(f"Labels encontrados: {labels}")

# Agregar labels al NER
for label in labels:
    ner.add_label(label)

# Dividir dataset en train (80%) y dev (20%)
random.shuffle(dataset)
split_point = int(len(dataset) * 0.8)
train_docs = dataset[:split_point]
dev_docs = dataset[split_point:]

print(f"Dataset: {len(dataset)} ejemplos")
print(f"Train: {len(train_docs)} ejemplos")
print(f"Dev: {len(dev_docs)} ejemplos")


# ---------------------------
# 3. FUNCIÓN DE EVALUACIÓN
# ---------------------------
def evaluate_model(nlp, examples):
    """Evalúa el modelo y retorna métricas"""
    scorer = nlp.evaluate(examples)
    return scorer


# Convertir datos a ejemplos de spaCy
def create_examples(nlp, data):
    examples = []
    for text, annotations in data:
        doc = nlp.make_doc(text)
        example = Example.from_dict(doc, annotations)
        examples.append(example)
    return examples


# ---------------------------
# 4. ENTRENAMIENTO
# ---------------------------
# Inicializar el entrenamiento
nlp.initialize(lambda: create_examples(nlp, train_docs))

print("\n" + "=" * 50)
print("COMENZANDO ENTRENAMIENTO")
print("=" * 50 + "\n")

n_iter = 30
dropout = 0.2

for i in range(n_iter):
    random.shuffle(train_docs)
    losses = {}

    # Crear ejemplos para esta iteración
    examples = create_examples(nlp, train_docs)

    # Entrenar en batches
    batches = minibatch(examples, size=compounding(4.0, 32.0, 1.001))
    for batch in batches:
        nlp.update(batch, drop=dropout, losses=losses)

    # Evaluar cada 5 iteraciones
    if (i + 1) % 5 == 0 or i == 0:
        dev_examples = create_examples(nlp, dev_docs)
        scores = evaluate_model(nlp, dev_examples)

        print(f"\n--- Iteración {i + 1}/{n_iter} ---")
        print(f"Pérdidas: {losses}")
        print(f"Precisión (P): {scores['ents_p']:.2f}")
        print(f"Recall (R): {scores['ents_r']:.2f}")
        print(f"F-Score (F1): {scores['ents_f']:.2f}")
        print(f"Per type: {scores['ents_per_type']}")
    else:
        print(f"Iteración {i + 1}/{n_iter} - Pérdidas: {losses}")

# ---------------------------
# 5. EVALUACIÓN FINAL
# ---------------------------
print("\n" + "=" * 50)
print("EVALUACIÓN FINAL")
print("=" * 50 + "\n")

dev_examples = create_examples(nlp, dev_docs)
final_scores = evaluate_model(nlp, dev_examples)

print(f"Precisión (P): {final_scores['ents_p']:.4f}")
print(f"Recall (R): {final_scores['ents_r']:.4f}")
print(f"F-Score (F1): {final_scores['ents_f']:.4f}")
print(f"\nMétricas por tipo:")
for entity_type, metrics in final_scores['ents_per_type'].items():
    print(f"  {entity_type}:")
    print(f"    P: {metrics['p']:.4f}")
    print(f"    R: {metrics['r']:.4f}")
    print(f"    F: {metrics['f']:.4f}")

# ---------------------------
# 6. GUARDAR MODELO
# ---------------------------
output_dir = "models/ner_v1/"
nlp.to_disk(output_dir)

print(f"\n✓ Modelo guardado en ./{output_dir}")
print(f"✓ Pipes en el modelo: {nlp.pipe_names}")
