import uvicorn
import os
from fastapi import FastAPI, File, UploadFile, HTTPException
from funasr import AutoModel
from funasr.utils.postprocess_utils import rich_transcription_postprocess

model_dir = "../SenseVoice/"

# asr model
model = AutoModel(
    model = model_dir + "iic/SenseVoiceSmall",
    trust_remote_code = True,
    remote_code = model_dir + "model.py",
    vad_model = "fsmn-vad",
    vad_kwargs = {"max_single_segment_time": 30000},
    device = "cuda:0",
    disable_update = True,
)

# dir
file_dir = os.path.dirname(__file__)


app = FastAPI()

@app.get("/")
async def hello():
    return "asr service on"

@app.post("/asr")
async def asr(file: UploadFile = File(...)):
    # check file
    if not file.filename.endswith(".wav"):
        raise HTTPException(status_code=400, detail="only wav file")

    save_path = os.path.join(file_dir, file.filename)

    try:
        # save wav file
        contents = await file.read()
        with open(save_path, "wb") as f:
            f.write(contents)
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"fail to save file: {str(e)}")

    res = model.generate(save_path, 
                        language="zn",  # "zn", "en", "yue", "ja", "ko", "nospeech"
                        use_itn=True,
                        batch_size_s=60,
                        merge_vad=True,
                        merge_length_s=15,)
    text = rich_transcription_postprocess(res[0]["text"])
    return text

if __name__ == '__main__':
    uvicorn.run(app, host='0.0.0.0', port=5000, reload=False)