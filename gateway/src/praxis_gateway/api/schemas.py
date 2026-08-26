from pydantic import BaseModel, ConfigDict


class Problem(BaseModel):
    """Stable error envelope that never leaks internal details."""

    model_config = ConfigDict(extra="forbid")

    code: str
    message: str
