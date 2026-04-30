import random
import sys
from pathlib import Path

import spacy
from spacy.training import Example
from spacy.util import compounding, minibatch

from data.generate_data import build_dataset

# Agregar el directorio raíz al path
sys.path.insert(0, str(Path(__file__).parent.parent))


# ---------------------------
# 1. CARGAR MODELO BASE
# ---------------------------
# Usamos es_core_news_md como punto de partida para aprovechar sus word vectors
# y el tok2vec preentrenado en español (Wikipedia + noticias).
#
# IMPORTANTE: solo actualizaremos el pipe "ner". Los demás pipes (tok2vec,
# morphologizer, etc.) se congelan para evitar catastrophic forgetting.
print("Cargando modelo base es_core_news_md...")
nlp = spacy.load("es_core_news_md")
print(f"Pipes disponibles en el modelo base: {nlp.pipe_names}")

# Obtener o añadir el pipe NER
if "ner" not in nlp.pipe_names:
    ner = nlp.add_pipe("ner", last=True)
else:
    ner = nlp.get_pipe("ner")

# ---------------------------
# 2. PREPARAR DATOS
# ---------------------------
dataset = build_dataset(seed=42)

labels = {label for _, ann in dataset for _, _, label in ann["entities"]}
print(f"\nLabels del dominio: {labels}")

# Añadir los labels custom al NER (no pisa los que ya tenía el modelo base)
for label in labels:
    ner.add_label(label)

random.shuffle(dataset)
split_point = int(len(dataset) * 0.8)
train_docs = dataset[:split_point]
dev_docs = dataset[split_point:]

print(f"Dataset total : {len(dataset)} ejemplos")
print(f"Train         : {len(train_docs)} ejemplos")
print(f"Dev           : {len(dev_docs)} ejemplos")


# ---------------------------
# 3. HELPERS
# ---------------------------
def create_examples(nlp, data):
    examples = []
    for text, annotations in data:
        doc = nlp.make_doc(text)
        example = Example.from_dict(doc, annotations)
        examples.append(example)
    return examples


def evaluate_model(nlp, examples):
    scorer = nlp.evaluate(examples)
    return scorer


# ---------------------------
# 4. ENTRENAMIENTO
# ---------------------------
# select_pipes(enable=["ner"]) congela todos los demás pipes durante el update.
# Así el tok2vec preentrenado NO se degrada con nuestros datos de dominio.
n_iter = 30
dropout = 0.2

print("\n" + "=" * 50)
print("COMENZANDO ENTRENAMIENTO (fine-tuning sobre es_core_news_md)")
print("=" * 50 + "\n")

with nlp.select_pipes(enable=["ner"]):
    optimizer = nlp.resume_training()

    for i in range(n_iter):
        random.shuffle(train_docs)
        losses = {}

        examples = create_examples(nlp, train_docs)
        batches = minibatch(examples, size=compounding(4.0, 32.0, 1.001))

        for batch in batches:
            nlp.update(batch, drop=dropout, losses=losses, sgd=optimizer)

        if (i + 1) % 5 == 0 or i == 0:
            dev_examples = create_examples(nlp, dev_docs)
            scores = evaluate_model(nlp, dev_examples)

            print(f"\n--- Iteración {i + 1}/{n_iter} ---")
            print(f"Pérdidas    : {losses}")
            print(f"Precisión P : {scores['ents_p']:.4f}")
            print(f"Recall    R : {scores['ents_r']:.4f}")
            print(f"F-Score  F1 : {scores['ents_f']:.4f}")
            print(f"Por tipo    : {scores['ents_per_type']}")
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

print(f"Precisión (P) : {final_scores['ents_p']:.4f}")
print(f"Recall    (R) : {final_scores['ents_r']:.4f}")
print(f"F-Score  (F1) : {final_scores['ents_f']:.4f}")
print(f"\nMétricas por tipo:")
for entity_type, metrics in final_scores["ents_per_type"].items():
    print(f"  {entity_type}:")
    print(f"    P : {metrics['p']:.4f}")
    print(f"    R : {metrics['r']:.4f}")
    print(f"    F : {metrics['f']:.4f}")


# ---------------------------
# 6. GUARDAR MODELO
# ---------------------------
output_dir = "models/ner_core_v1/"
nlp.to_disk(output_dir)

print(f"\n✓ Modelo guardado en ./{output_dir}")
print(f"✓ Pipes en el modelo guardado: {nlp.pipe_names}")
