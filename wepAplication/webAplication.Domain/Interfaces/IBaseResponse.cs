﻿namespace webAplication.Domain.Interfaces;

public interface IBaseResponse<T>
{
    string Description { get; set; }
    StatusCode StatusCode { get; set; }
    T Data { get; set; }
}