﻿namespace Montage.Card.API.Services;

public interface IFileOutCommandProcessor
{
    public Task Process(string fullOutCommand, string fullFilePath, CancellationToken cancellationToken = default);
}
