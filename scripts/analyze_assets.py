import os
import json
from pathlib import Path
from datetime import datetime

def analyze_assets():
    """Analiza todos los assets del proyecto Unity"""
    
    assets_folder = "Assets"
    metrics = {
        "prefabs": [],
        "scenes": [],
        "meshes": [],
        "textures": [],
        "materials": [],
        "audio": [],
        "scripts": [],
        "otros": []
    }
    
    total_size = 0
    
    # Mapeo de extensiones
    extensions = {
        ".prefab": "prefabs",
        ".unity": "scenes",
        ".fbx": "meshes",
        ".obj": "meshes",
        ".blend": "meshes",
        ".png": "textures",
        ".jpg": "textures",
        ".jpeg": "textures",
        ".tga": "textures",
        ".mat": "materials",
        ".mp3": "audio",
        ".wav": "audio",
        ".ogg": "audio",
        ".m4a": "audio",
        ".cs": "scripts",
        ".shader": "materials",
    }
    
    if not os.path.exists(assets_folder):
        print(f"⚠️ Carpeta {assets_folder} no encontrada")
        # Crear estructura de prueba si no existe
        os.makedirs(assets_folder, exist_ok=True)
        return
    
    print(f"📂 Analizando carpeta: {assets_folder}")
    
    # Recorrer todos los archivos
    for root, dirs, files in os.walk(assets_folder):
        # Ignorar carpetas de meta
        dirs[:] = [d for d in dirs if not d.startswith('.')]
        
        for file in files:
            # Ignorar archivos .meta de Unity
            if file.endswith('.meta'):
                continue
                
            file_path = os.path.join(root, file)
            
            try:
                file_size = os.path.getsize(file_path)
                file_ext = os.path.splitext(file)[1].lower()
                
                total_size += file_size
                
                # Clasificar archivo
                asset_type = "otros"
                for ext, type_name in extensions.items():
                    if file_ext == ext:
                        asset_type = type_name
                        break
                
                relative_path = os.path.relpath(file_path, assets_folder)
                
                metrics[asset_type].append({
                    "name": file,
                    "path": relative_path,
                    "size_bytes": file_size,
                    "size_kb": round(file_size / 1024, 2),
                    "size_mb": round(file_size / (1024**2), 2)
                })
            except Exception as e:
                print(f"⚠️ Error al procesar {file}: {e}")
    
    # Crear reporte
    os.makedirs("metrics", exist_ok=True)
    
    report = {
        "timestamp": datetime.now().isoformat(),
        "date": datetime.now().strftime("%Y-%m-%d %H:%M:%S"),
        "total_size_bytes": total_size,
        "total_size_kb": round(total_size / 1024, 2),
        "total_size_mb": round(total_size / (1024**2), 2),
        "total_size_gb": round(total_size / (1024**3), 2),
        "metrics": {}
    }
    
    # Procesar cada tipo de asset
    for asset_type, assets in metrics.items():
        if assets:
            type_size = sum(a["size_bytes"] for a in assets)
            assets_sorted = sorted(assets, key=lambda x: x["size_bytes"], reverse=True)
            
            report["metrics"][asset_type] = {
                "count": len(assets),
                "total_size_bytes": type_size,
                "total_size_kb": round(type_size / 1024, 2),
                "total_size_mb": round(type_size / (1024**2), 2),
                "percentage": round((type_size / total_size * 100) if total_size > 0 else 0, 2),
                "top_10": [
                    {
                        "name": a["name"],
                        "path": a["path"],
                        "size_kb": a["size_kb"],
                        "size_mb": a["size_mb"]
                    } for a in assets_sorted[:10]
                ],
                "all_files": [
                    {
                        "name": a["name"],
                        "path": a["path"],
                        "size_mb": a["size_mb"]
                    } for a in assets_sorted
                ]
            }
    
    # Guardar reporte JSON
    with open("metrics/assets_report.json", "w", encoding="utf-8") as f:
        json.dump(report, f, indent=2, ensure_ascii=False)
    
    # Imprimir resumen
    print("\n" + "="*60)
    print("📊 RESUMEN DE ANÁLISIS")
    print("="*60)
    print(f"📦 Tamaño Total: {report['total_size_mb']:.2f} MB ({report['total_size_gb']:.2f} GB)")
    print(f"📁 Tipos de Assets: {len([m for m in report['metrics'].values() if m['count'] > 0])}")
    print(f"📄 Total de Archivos: {sum(m['count'] for m in report['metrics'].values())}")
    print("\n📈 Por Tipo:")
    
    for asset_type, data in report["metrics"].items():
        if data["count"] > 0:
            print(f"  • {asset_type.upper():15} - {data['count']:5} archivos - {data['total_size_mb']:8.2f} MB ({data['percentage']:5.1f}%)")
    
    print("="*60 + "\n")
    
    return report

if __name__ == "__main__":
    analyze_assets()