import os
import zipfile
from datetime import datetime

def create_zip_report():
    """Crea un ZIP con las gráficas y reporte JSON"""
    
    print("\n" + "="*60)
    print("📦 CREANDO ZIP DE REPORTE")
    print("="*60)
    
    metrics_folder = "metrics"
    
    if not os.path.exists(metrics_folder):
        print("⚠️ Carpeta metrics no encontrada")
        return
    
    zip_name = f"metrics_report.zip"
    
    try:
        with zipfile.ZipFile(zip_name, 'w', zipfile.ZIP_DEFLATED) as zipf:
            for root, dirs, files in os.walk(metrics_folder):
                for file in files:
                    file_path = os.path.join(root, file)
                    arcname = os.path.relpath(file_path, '.')
                    zipf.write(file_path, arcname)
                    print(f"  ✓ {file}")
        
        zip_size_mb = os.path.getsize(zip_name) / (1024**2)
        
        print(f"\n✅ ZIP creado: {zip_name} ({zip_size_mb:.2f} MB)")
        print("="*60 + "\n")
        
    except Exception as e:
        print(f"❌ Error: {e}")

if __name__ == "__main__":
    create_zip_report()